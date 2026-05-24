using UnityEngine;

public abstract class PlayerCameraBaseState : IPlayerCameraState
{
    protected readonly PlayerCameraStateMachine _stateMachine;
    protected readonly ICameraMotor _cameraMotor;
    protected readonly IInputRouter _router;


    public PlayerCameraBaseState(PlayerCameraStateMachine stateMachine,
                                 ICameraMotor cameraMotor,
                                 IInputRouter router)
    {
        _stateMachine = stateMachine;
        _cameraMotor = cameraMotor;
        _router = router;
    }

    public virtual void Enter()
    {
        if(this is IGameplayInputReceiver gmState)
        {
            _router.Subscribe(InputMode.Gameplay, gmState);
        }

        if(this is IInspectInputReceiver insState)
        {
            _router.Subscribe(InputMode.InspectExhibit, insState);
        }
    }

    public virtual void Exit()
    {
        if(this is IGameplayInputReceiver gmState)
        {
            _router.Unsubscribe(InputMode.Gameplay, gmState);
        }

        if(this is IInspectInputReceiver insState)
        {
            _router.Unsubscribe(InputMode.InspectExhibit, insState);
        }
    }

    public virtual void LateTick()
    {
    }

    public virtual void Tick()
    {
    }

    public virtual void FixedTick()
    {
    }
}
