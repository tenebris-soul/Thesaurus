using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class PlayerLocomotionStateMachine : IPlayerLocomotionStateMachine, ITickable, IFixedTickable
{
    private IPlayerLocomotionState _currentState;
    private List<IPlayerLocomotionState> _states;

    public PlayerLocomotionStateMachine(IPlayerMotor motor,
                                        PlayerMovementConfig config,
                                        PlayerReadContext playerReadContext,
                                        PlayerWriteContext playerWriteContext,
                                        SignalBus bus,
                                        IInputRouter router)
    {
        _states = new List<IPlayerLocomotionState>()
        {
            new PlayerLocomotionGroundedState(this, motor, playerReadContext, playerWriteContext, config, bus, router),
            new PlayerLocomotionAirborneState(this, motor, playerReadContext, playerWriteContext, config, bus, router),
            new PlayerLocomotionSlidingState(this, motor, playerReadContext, config, bus, router),
            new PlayerLocomotionBlockedState(this, motor, bus, router)
        };

        _currentState = _states[0];
        _currentState.Enter();
    }

    public void SwitchState<T>() where T : IPlayerLocomotionState
    {
        IPlayerLocomotionState newState = _states.FirstOrDefault(state => state is T);

        if (newState == null)
        {
            Debug.LogWarning("There is no state such as " + nameof(T));
            return;
        }

        _currentState.Exit();
        _currentState = newState;
        _currentState.Enter();
    }

    public void Tick()
    {
        _currentState.Tick();
    }

    public void FixedTick()
    {
        _currentState.FixedTick();
    }
}
