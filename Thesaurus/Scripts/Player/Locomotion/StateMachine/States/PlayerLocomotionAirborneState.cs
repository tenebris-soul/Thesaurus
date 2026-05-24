using UnityEngine;
using UnityEngine.Timeline;
using Zenject;

public class PlayerLocomotionAirborneState : PlayerLocomotionBaseState, IGameplayInputReceiver
{
    private IPlayerMotor _motor;

    private float _verticalVelocity;

    private PlayerMovementConfig _config;
    private PlayerReadContext _playerReadContext;
    private PlayerWriteContext _playerWriteContext;

    private SignalBus _bus;

    private GameplayInputFrame _frame;

    public PlayerLocomotionAirborneState(
        IPlayerLocomotionStateMachine stateMachine,
        IPlayerMotor motor,
        PlayerReadContext playerReadContext,
        PlayerWriteContext playerWriteContext,
        PlayerMovementConfig config,
        SignalBus bus,
        IInputRouter router)
        : base(stateMachine, router)
    {
        _motor = motor;
        _config = config;
        _playerReadContext = playerReadContext;
        _playerWriteContext = playerWriteContext;
        _bus = bus;
    }

    public override void Enter()
    {
        base.Enter();

        SyncVerticalFromMotorVelocity();

        FireLeftGroundEvents();

        if (_playerReadContext.RequestJumping)
        {
            ApplyJumpImpulse();
            FireJumpingEvents();

            _playerWriteContext.SetRequestJumping(false);
        }

        // Debug.Log("In air");
    }

    public override void FixedTick()
    {
        base.FixedTick();

        ReadGround(out bool isGrounded, out bool isSteepGround, out bool isHittedCeiling);

        bool isFalling = _verticalVelocity >= 0f;

        if (isFalling && isGrounded && !isSteepGround)
        {
            _stateMachine.SwitchState<PlayerLocomotionGroundedState>();
            return;
        }
        else if (isFalling && isGrounded && isSteepGround)
        {
            _stateMachine.SwitchState<PlayerLocomotionSlidingState>();
            return;
        }

        float dt = Time.fixedDeltaTime;

        Vector3 currentVelocity = _motor.Velocity;

        UpdateVerticalVelocity(ref currentVelocity, isHittedCeiling, dt);

        Vector3 horizVel = ExtractHorizontal(currentVelocity);

        Vector3 wishFlat;
        float wishMag;
        ReadAirInput(out wishFlat, out wishMag);

        ApplyAirControl(ref horizVel, wishFlat, wishMag, dt);

        currentVelocity = ComposeVelocity(horizVel, _verticalVelocity);

        _motor.SetVelocity(currentVelocity);
    }

    public override void Exit()
    {
        base.Exit();
        _verticalVelocity = 0f;

        FireLandingEvents();
    }

    public void OnInput(in GameplayInputFrame frame)
    {
        _frame = frame;
    }

    private void ReadGround(out bool isGrounded, out bool isSteepGround, out bool isHittedCeiling)
    {
        isGrounded = _playerReadContext.IsGrounded;
        isSteepGround = _playerReadContext.IsSteepGround;
        isHittedCeiling = _playerReadContext.IsHittingCeiling;
    }

    private void SyncVerticalFromMotorVelocity()
    {
        _verticalVelocity = -_motor.Velocity.y;
    }

    private void ApplyJumpImpulse()
    {
        float gravity = _config.Gravity;
        float jumpHeight = _config.JumpHeight;

        _verticalVelocity = -Mathf.Sqrt(2f * gravity * jumpHeight);

        Vector3 v = _motor.Velocity;
        v.y = -_verticalVelocity;
        _motor.SetVelocity(v);
    }

    private void UpdateVerticalVelocity(ref Vector3 currentVelocity, bool isHittedCeiling, float dt)
    {
        float gravity = _config.Gravity;

        _verticalVelocity = -currentVelocity.y;

        if (isHittedCeiling && _verticalVelocity < 0f)
            _verticalVelocity = 0f;

        _verticalVelocity += gravity * dt;
    }

    private static Vector3 ExtractHorizontal(Vector3 v)
    {
        return new Vector3(v.x, 0f, v.z);
    }

    private void ReadAirInput(out Vector3 wishFlat, out float wishMag)
    {
        Vector3 inputMove = _frame.Move;
        (Vector3 forward, Vector3 right) = _motor.GetPlayerDirs();

        Vector3 wish = (forward * inputMove.y + right * inputMove.x);
        wishMag = Mathf.Clamp01(wish.magnitude);

        if (wishMag > 1e-4f)
            wish /= wishMag;

        wishFlat = Vector3.ProjectOnPlane(wish, Vector3.up);
        if (wishFlat.sqrMagnitude > 1e-6f)
            wishFlat.Normalize();
        else
            wishFlat = Vector3.zero;
    }

    private void ApplyAirControl(ref Vector3 horizVel, Vector3 wishFlat, float wishMag, float dt)
    {
        float airControlSpeed = _config.AirControlSpeed;
        float airAccel = _config.AirAcceleration;
        float airTurnRateDeg = _config.AirTurnRateDegrees;
        float turnSlowdown = _config.TurnSlowdown;

        float speed = horizVel.magnitude;
        if (wishFlat.sqrMagnitude > 1e-6f)
        {
            float targetSpeed = Mathf.Max(speed, airControlSpeed * wishMag);

            float turnRate = airTurnRateDeg / (1f + speed * turnSlowdown);
            float maxRad = turnRate * Mathf.Deg2Rad * dt;

            Vector3 targetVel = wishFlat * targetSpeed;

            horizVel = Vector3.RotateTowards(horizVel, targetVel, maxRad, airAccel * dt);
        }
    }

    private static Vector3 ComposeVelocity(Vector3 horizVel, float verticalVelocity)
    {
        return new Vector3(horizVel.x, -verticalVelocity, horizVel.z);
    }

    private void FireLeftGroundEvents()
    {
        SearchModeSignal signal = new() { IsActive = false };
        _bus.Fire(signal);   
    }

    private void FireLandingEvents()
    {
        LandingEndedSignal landSignal = new LandingEndedSignal();
        _bus.Fire(landSignal);   

        SearchModeSignal searchSignal = new() { IsActive = true };
        _bus.Fire(searchSignal);        
    }

    private void FireJumpingEvents()
    {
        JumpingStartedSignal signal = new JumpingStartedSignal();
        _bus.Fire(signal);
    }

}
