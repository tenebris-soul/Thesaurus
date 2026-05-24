using UnityEngine;
using Zenject;

public class PlayerLocomotionGroundedState : PlayerLocomotionBaseState, IGameplayInputReceiver
{
    private IPlayerMotor _motor;

    private PlayerMovementConfig _config;

    private PlayerReadContext _playerReadContext;
    private PlayerWriteContext _playerWriteContext;

    private Vector2 _inputMove;

    private SignalBus _bus;

    private GameplayInputFrame _frame;

    public PlayerLocomotionGroundedState(IPlayerLocomotionStateMachine stateMachine, 
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
        _bus = bus;
        _playerReadContext = playerReadContext;
        _playerWriteContext = playerWriteContext;

        _bus.Subscribe<InspectModeSignal>(SwitchToInspect);
        _bus.Subscribe<ArtefactModeSignal>(SwitchToArtefact);
        _bus.Subscribe<PaintingModeSignal>(SwitchToPainting);
    }

    public override void Enter()
    {
        base.Enter(); 
    }

    public override void Tick()
    {
        base.Tick();

        _inputMove = GetInput();

        if(_inputMove != Vector2.zero)
        {
            float motorSpeed = _motor.Velocity.magnitude;
            WalkingSignal signal = new WalkingSignal() { HorizontalSpeed = motorSpeed };

            _bus.Fire(signal);
        } 
        else
        {
            StandingSignal signal = new StandingSignal();
            _bus.Fire(signal);
        }

        if(_frame.IsJumping)
        {
            _playerWriteContext.SetRequestJumping(true);
        }
    }

    public override void FixedTick()
    {
        base.FixedTick();

        ReadGround(out bool isGrounded, out bool isSteepGround, out Vector3 groundNormal);

        bool requestedJump = _playerReadContext.RequestJumping;

        if (!isGrounded || requestedJump)
        {
            _stateMachine.SwitchState<PlayerLocomotionAirborneState>();
            return;
        }    
        else if (isGrounded && isSteepGround)
        {
            _stateMachine.SwitchState<PlayerLocomotionSlidingState>();
            return;
        }

        (Vector3 forward, Vector3 right) = _motor.GetPlayerDirs();

        Vector3 desiredDir = GetDesiredDir(forward, right);

        float speed = 0f;
        SetSpeed(ref speed, _inputMove);

        Vector3 realDir = GetRealDir(isGrounded, isSteepGround, groundNormal, desiredDir);

        Vector3 planarVelocity = realDir * speed;

        _motor.SetVelocity(planarVelocity);
    }

    public override void Exit()
    {
        base.Exit();

        StandingSignal signal = new StandingSignal();
        _bus.Fire(signal);
    }

    public void OnInput(in GameplayInputFrame frame)
    {
        _frame = frame;
    }

    private void ReadGround(out bool isGrounded, out bool isSteepGround, out Vector3 groundNormal)
    {
        isGrounded = _playerReadContext.IsGrounded;
        isSteepGround = _playerReadContext.IsSteepGround;
        groundNormal = _playerReadContext.GroundNormal;
    }

    private void SetSpeed(ref float speed, Vector2 inputMove)
    {
        if (inputMove.y > 0)
        {
            speed = _config.ForwardSpeed;
        }
        else if (inputMove.y < 0 || inputMove.x != 0)
        {
            speed = _config.BackSpeed;
        }
    }

    private Vector2 GetInput()
    {
        Vector2 inputMove = _frame.Move;

        if (inputMove.sqrMagnitude < 0.01f) 
            inputMove = Vector2.zero;

        return inputMove;
    }

    private Vector3 GetDesiredDir(in Vector3 forward, in Vector3 right)
    {
        Vector3 desired = forward * _inputMove.y + right * _inputMove.x;
        float desiredMag = desired.magnitude;

        return (desiredMag > 1e-6f) ? (desired / desiredMag) : Vector3.zero;
    }

    private Vector3 GetRealDir(in bool isGrounded, 
                               in bool isSteepGround, 
                               in Vector3 groundNormal, 
                               in Vector3 desiredDir)
    {
        return (isGrounded && !isSteepGround)
                          ? Vector3.ProjectOnPlane(desiredDir, groundNormal).normalized
                          : Vector3.ProjectOnPlane(desiredDir, Vector3.up).normalized;
    }

    private void SwitchToInspect(InspectModeSignal signal)
    {
        bool isActive = signal.IsActive;

        if(!isActive) return;

        _stateMachine.SwitchState<PlayerLocomotionBlockedState>();
    }

    private void SwitchToArtefact(ArtefactModeSignal signal)
    {
        bool isActive = signal.IsActive;

        if(!isActive) return;

        _stateMachine.SwitchState<PlayerLocomotionBlockedState>();
    }
    private void SwitchToPainting(PaintingModeSignal signal)
    {
        bool isActive = signal.IsActive;

        if(!isActive) return;

        _stateMachine.SwitchState<PlayerLocomotionBlockedState>();
    }
}
