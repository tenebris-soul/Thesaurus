using UnityEngine;
using Zenject;

public class PlayerCameraInspectState : PlayerCameraBaseState, IInspectInputReceiver
{
    private readonly SignalBus _bus;
    
    private InspectInputFrame _frame;
    public PlayerCameraInspectState(PlayerCameraStateMachine stateMachine, 
                                    ICameraMotor cameraMotor,
                                    IInputRouter router,
                                    SignalBus bus) 
                                    : base(stateMachine, cameraMotor, router)
    {
        _bus = bus;
        _bus.Subscribe<InspectModeSignal>(SwitchToSearch);
        _bus.Subscribe<ArtefactModeSignal>(SwitchToSearch);
        _bus.Subscribe<PaintingModeSignal>(SwitchToSearch);
    }

    public override void Enter()
    {
        base.Enter();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public void OnInput(in InspectInputFrame frame)
    {
        _frame = frame;
    }

    private void SwitchToSearch(InspectModeSignal signal)
    {
        bool isActive = signal.IsActive;

        if(isActive) return;

        _cameraMotor.ReleaseChangeTransform();
        _stateMachine.SwitchState<PlayerCameraLookState>();
    }

    private void SwitchToSearch(ArtefactModeSignal signal)
    {
        bool isActive = signal.IsActive;

        if(isActive) return;

        _cameraMotor.ReleaseChangeTransform();
        _stateMachine.SwitchState<PlayerCameraLookState>();
    }

    private void SwitchToSearch(PaintingModeSignal signal)
    {
        bool isActive = signal.IsActive;

        if(isActive) return;

        _cameraMotor.ReleaseChangeTransform();
        _stateMachine.SwitchState<PlayerCameraLookState>();
    }
}
