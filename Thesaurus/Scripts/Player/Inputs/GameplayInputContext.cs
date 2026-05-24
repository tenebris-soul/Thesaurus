using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class GameplayInputContext : IInputContext<IGameplayInputReceiver>
{
    public string ActionMap => "Gameplay";
    private readonly InputActionMap _map;
    private readonly List<IGameplayInputReceiver> _subs = new();

    private readonly InputAction _move;
    private readonly InputAction _look;
    private readonly InputAction _jump;
    private readonly InputAction _interact;
    private readonly InputAction _pause;

    private const string MOVE_ACTION = "Move";
    private const string LOOK_ACTION = "Look";
    private const string JUMP_ACTION = "Jump";
    private const string INTERACT_ACTION = "Interact";
    private const string PAUSE_ACTION = "Pause";

    public GameplayInputContext(InputActionAsset asset
                                // List<IGameplayInputReceiver> subs
                                )
    {
        _map = asset.FindActionMap(ActionMap, true);

        _move = _map.FindAction(MOVE_ACTION, true);
        _look = _map.FindAction(LOOK_ACTION, true);
        _jump = _map.FindAction(JUMP_ACTION, true);
        _interact = _map.FindAction(INTERACT_ACTION, true);
        _pause = _map.FindAction(PAUSE_ACTION, true);

        // _subs = subs;
    }

    public void Disable() => _map.Disable();

    public void Enable() => _map.Enable();

    public void ReadAndDispatch()
    {
        var move = _move.ReadValue<Vector2>();
        var look = _look.ReadValue<Vector2>();
        var jump = _jump.WasPressedThisFrame();
        var interact = _interact.WasPressedThisFrame();
        var pause = _pause.WasPressedThisFrame();

        var frame = new GameplayInputFrame(move, look, jump, interact, pause);

        for(int i = 0; i < _subs.Count; i++)
        {
            var sub = _subs[i];
            sub.OnInput(frame);
        }
    }

    public void Subscribe(IGameplayInputReceiver receiver)
    {
        if(_subs.Contains(receiver))
        {
            Debug.LogWarning("Receiver is already subscribed!");
            return;
        }

        _subs.Add(receiver);
    }

    public void Unsubscribe(IGameplayInputReceiver receiver)
    {
        if(!_subs.Contains(receiver))
        {
            Debug.LogWarning("There is no such receivers!");
            return;
        }

        _subs.Remove(receiver);
    }
}