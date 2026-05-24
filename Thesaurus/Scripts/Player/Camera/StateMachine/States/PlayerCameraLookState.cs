using UnityEngine;
using Zenject;

public class PlayerCameraLookState : PlayerCameraBaseState, IGameplayInputReceiver
{
    private readonly SignalBus _bus;

    private GameplayInputFrame _frame;

    public PlayerCameraLookState(PlayerCameraStateMachine stateMachine,
                                 ICameraMotor cameraMotor,
                                 IInputRouter router,
                                 SignalBus bus) 
                          : base(stateMachine, cameraMotor, router)
    {
        _bus = bus;

        _bus.Subscribe<InspectModeSignal>(SwitchToInspect);
        _bus.Subscribe<ArtefactModeSignal>(SwitchToArtefactInspect);
        _bus.Subscribe<PaintingModeSignal>(SwitchToPaintingInspect);
    }

    public override void Enter()
    {
        base.Enter();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public override void Tick()
    {
        base.Tick();

        _cameraMotor.AddYawPitch(_frame.LookDelta);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public void OnInput(in GameplayInputFrame frame)
    {
        _frame = frame;
    }

    private void SwitchToInspect(InspectModeSignal signal)
    {
        bool isActive = signal.IsActive;

        if(!isActive) return;

        _cameraMotor.BlockChangeTransform();
        _stateMachine.SwitchState<PlayerCameraInspectState>();
    }

    private void SwitchToArtefactInspect(ArtefactModeSignal signal)
    {
        bool isActive = signal.IsActive;

        if(!isActive) return;

        _cameraMotor.BlockChangeTransform();
        _stateMachine.SwitchState<PlayerCameraInspectState>();
    }

    private void SwitchToPaintingInspect(PaintingModeSignal signal)
    {
        bool isActive = signal.IsActive;

        if(!isActive) return;

        _cameraMotor.BlockChangeTransform();
        _stateMachine.SwitchState<PlayerCameraInspectState>();
    }
}
