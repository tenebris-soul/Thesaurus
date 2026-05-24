using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public abstract class PlayerLocomotionBaseState : IPlayerLocomotionState
{
    protected IPlayerLocomotionState _currentSubstate;
    protected List<IPlayerLocomotionState> _substates;
    protected IPlayerLocomotionStateMachine _stateMachine;
    
    protected readonly IInputRouter _router;

    public PlayerLocomotionBaseState(IPlayerLocomotionStateMachine stateMachine,
                                     IInputRouter router)
    {
        _stateMachine = stateMachine;
        _router = router;
    }

    public virtual void Enter()
    {
        if(this is IGameplayInputReceiver gmState)
        {
            _router.Subscribe(InputMode.Gameplay, gmState);
        }
    }

    public virtual void Exit()
    {
        if(this is IGameplayInputReceiver gmState)
        {
            _router.Unsubscribe(InputMode.Gameplay, gmState);
        }
    }

    public virtual void FixedTick()
    {
        _currentSubstate?.FixedTick();
    }

    public virtual void Tick()
    {
        _currentSubstate?.Tick();
    }

    public void SwitchSubstate<T>() where T : IPlayerLocomotionState
    {
        IPlayerLocomotionState newState = _substates.FirstOrDefault(state => state is T);

        if (newState == null)
        {
            Debug.LogWarning("There is no state such as " + nameof(T));
            return;
        }

        _currentSubstate.Exit();
        _currentSubstate = newState;
        _currentSubstate.Enter();
    }
}
