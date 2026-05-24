using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;

public class PlayerCameraStateMachine : IPlayerCameraStateMachine, ITickable, ILateTickable, IFixedTickable
{
    private IPlayerCameraState _currentState;
    private List<IPlayerCameraState> _states;

    private readonly ICameraMotor _cameraMotor;

    public PlayerCameraStateMachine(ICameraMotor cameraMotor,
                                    SignalBus bus,
                                    IInputRouter router)
    {
        _cameraMotor = cameraMotor;

        _states = new List<IPlayerCameraState>()
        {
            new PlayerCameraLookState(this, _cameraMotor, router, bus),
            new PlayerCameraInspectState(this, _cameraMotor, router, bus)
        };

        _currentState = _states[0]; // потом поменять
        _currentState.Enter();
    }

    public void SwitchState<T>() where T : IPlayerCameraState
    {
        IPlayerCameraState newState = _states.FirstOrDefault(state => state is T);

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

    public void LateTick()
    {
        _currentState.LateTick();
    }

    public void FixedTick()
    {
        _currentState.FixedTick();
    }
}
