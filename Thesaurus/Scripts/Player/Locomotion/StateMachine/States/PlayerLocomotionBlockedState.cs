using UnityEngine;
using Zenject;

public class PlayerLocomotionBlockedState : PlayerLocomotionBaseState, IInspectInputReceiver
{
    private readonly SignalBus _bus;
    private readonly IPlayerMotor _motor;

    private InspectInputFrame _frame;

    public PlayerLocomotionBlockedState(
        IPlayerLocomotionStateMachine stateMachine,
        IPlayerMotor motor,
        SignalBus bus,
        IInputRouter router) : base(stateMachine, router)
    {
        _bus = bus;
        _motor = motor;

        _bus.Subscribe<InspectModeSignal>(SwitchToSearch);
        _bus.Subscribe<ArtefactModeSignal>(SwitchToSearch);
        _bus.Subscribe<PaintingModeSignal>(SwitchToSearch);
    }

    public override void Enter()
    {
        base.Enter();
        _motor.SetVelocity(Vector3.zero);
    }

    public override void FixedTick()
    {
    }

    public override void Exit()
    {
        base.Exit();
        // _bus.Unsubscribe<InspectModeSignal>(SwitchToSearch);
    }

    public void OnInput(in InspectInputFrame frame)
    {
        _frame = frame;
    }

    private void SwitchToSearch(InspectModeSignal signal)
    {
        bool isActive = signal.IsActive;

        if(isActive) return;

        _stateMachine.SwitchState<PlayerLocomotionGroundedState>();
    }
    private void SwitchToSearch(ArtefactModeSignal signal)
    {
        bool isActive = signal.IsActive;

        if(isActive) return;

        _stateMachine.SwitchState<PlayerLocomotionGroundedState>();
    }
    private void SwitchToSearch(PaintingModeSignal signal)
    {
        bool isActive = signal.IsActive;

        if(isActive) return;

        _stateMachine.SwitchState<PlayerLocomotionGroundedState>();
    }
}
