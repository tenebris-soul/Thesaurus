using UnityEngine;
using Zenject;

public class PlayerLocomotionSlidingState : PlayerLocomotionBaseState, IGameplayInputReceiver
{
    private IPlayerMotor _motor;
    private PlayerMovementConfig _config;
    private PlayerReadContext _playerReadContext;

    private SignalBus _bus;

    private GameplayInputFrame _frame;

    public PlayerLocomotionSlidingState(
        IPlayerLocomotionStateMachine stateMachine,
        IPlayerMotor motor,
        PlayerReadContext playerReadContext,
        PlayerMovementConfig config,
        SignalBus bus,
        IInputRouter router) : base(stateMachine, router)
    {
        _motor = motor;
        _config = config;
        _playerReadContext = playerReadContext;
        _bus = bus;
    }

    public override void Enter()
    {
        base.Enter();

        // Debug.Log("Sliding");
    }

    public override void FixedTick()
    {
        base.FixedTick();

        ReadGround(out bool isGrounded, out bool isSteepGround, out Vector3 groundNormal);
        
        if (isGrounded && !isSteepGround)
        {
            _stateMachine.SwitchState<PlayerLocomotionGroundedState>();
            return;
        }
        else if (!isGrounded)
        {
            _stateMachine.SwitchState<PlayerLocomotionAirborneState>();
            return;
        }

        float dt = Time.fixedDeltaTime;

        ReadSlideConfig(
            out float slideGravityMul,
            out float slideDrag,
            out float slideMaxSpeed,
            out float slideStrafeMaxSpeed,
            out float slideStrafeAccel);

        Vector3 vOnPlane = GetVelocityOnGroundPlane(groundNormal);

        Vector3 slideDir = GetSlideDir(groundNormal);

        ApplySlopeAcceleration(ref vOnPlane, groundNormal, dt, slideGravityMul);

        Vector3 strafeAxis = GetStrafeAxis(groundNormal, slideDir);

        ApplyStrafeControl(
            ref vOnPlane,
            strafeAxis,
            _frame.Move.x,
            slideStrafeMaxSpeed,
            slideStrafeAccel,
            dt);


        ApplyDragAndClamp(ref vOnPlane, slideDrag, slideMaxSpeed, dt);

        _motor.SetVelocity(vOnPlane);

        FireSlidingEvents();
    }

    public override void Exit()
    {
        base.Exit();

        FireSlidingEndEvents();
    }

    private void ReadGround(out bool isGrounded, out bool isSteepGround, out Vector3 groundNormal)
    {
        isGrounded = _playerReadContext.IsGrounded;
        isSteepGround = _playerReadContext.IsSteepGround;
        groundNormal = _playerReadContext.GroundNormal;
    }

    private void ReadSlideConfig(
        out float slideGravityMul,
        out float slideDrag,
        out float slideMaxSpeed,
        out float slideStrafeMaxSpeed,
        out float slideStrafeAccel)
    {
        slideGravityMul = _config.SlideGravityMul;
        slideDrag = _config.SlideDrag;
        slideMaxSpeed = _config.SlideMaxSpeed;
        slideStrafeMaxSpeed = _config.SlideStrafeMaxSpeed;
        slideStrafeAccel = _config.SlideStrafeAccel;
    }

    private Vector3 GetVelocityOnGroundPlane(Vector3 groundNormal)
    {
        Vector3 currentVelocity = _motor.Velocity;
        return Vector3.ProjectOnPlane(currentVelocity, groundNormal);
    }

    private static Vector3 GetSlideDir(Vector3 groundNormal)
    {
        Vector3 slideDir = Vector3.ProjectOnPlane(Vector3.down, groundNormal);
        if (slideDir.sqrMagnitude > 1e-6f)
            slideDir.Normalize();
        else
            slideDir = Vector3.zero;

        return slideDir;
    }

    private void ApplySlopeAcceleration(ref Vector3 vOnPlane, Vector3 groundNormal, float dt, float slideGravityMul)
    {
        Vector3 gravityVec = Vector3.down * _config.Gravity;
        Vector3 slopeAccel = Vector3.ProjectOnPlane(gravityVec, groundNormal) * slideGravityMul;
        vOnPlane += slopeAccel * dt;
    }

    private Vector3 GetStrafeAxis(Vector3 groundNormal, Vector3 slideDir)
    {
        (_, Vector3 right) = _motor.GetPlayerDirs();

        Vector3 rightOnSlope = Vector3.ProjectOnPlane(right, groundNormal);
        if (rightOnSlope.sqrMagnitude > 1e-6f)
            rightOnSlope.Normalize();
        else
            return Vector3.zero;

        Vector3 strafeAxis = rightOnSlope;
        if (slideDir != Vector3.zero)
        {
            strafeAxis -= slideDir * Vector3.Dot(strafeAxis, slideDir);
            if (strafeAxis.sqrMagnitude > 1e-6f)
                strafeAxis.Normalize();
            else
                strafeAxis = Vector3.zero;
        }

        return strafeAxis;
    }

    private static void ApplyStrafeControl(
        ref Vector3 vOnPlane,
        Vector3 strafeAxis,
        float inputX,
        float strafeMaxSpeed,
        float strafeAccel,
        float dt)
    {
        if (strafeAxis == Vector3.zero)
            return;

        float curSide = Vector3.Dot(vOnPlane, strafeAxis);
        float targetSide = inputX * strafeMaxSpeed;
        float newSide = Mathf.MoveTowards(curSide, targetSide, strafeAccel * dt);

        vOnPlane += strafeAxis * (newSide - curSide);
    }

    private static void ApplyDragAndClamp(ref Vector3 vOnPlane, float slideDrag, float slideMaxSpeed, float dt)
    {
        vOnPlane *= 1f / (1f + slideDrag * dt);
        vOnPlane = Vector3.ClampMagnitude(vOnPlane, slideMaxSpeed);
    }

    private void FireSlidingEvents()
    {
        SlidingSignal signal = new();
        _bus.Fire(signal);
    }

    private void FireSlidingEndEvents()
    {
        SlidingEndedSignal signal = new();
        _bus.Fire(signal);
    }

    public void OnInput(in GameplayInputFrame frame)
    {
        _frame = frame;
    }
}
