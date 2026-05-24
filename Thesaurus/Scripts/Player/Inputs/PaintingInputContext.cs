using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PaintingInputContext : IInputContext<IPaintingInputReceiver>
{
    public string ActionMap => "InspectPainting";
    private readonly InputActionMap _map;
    private readonly List<IPaintingInputReceiver> _subs = new();

    private readonly InputAction _movePressed;
    private readonly InputAction _moveDelta;
    private readonly InputAction _zoom;
    private readonly InputAction _cancelPressed;

    public PaintingInputContext(InputActionAsset asset)
    {
        _map = asset.FindActionMap(ActionMap, true);

        _movePressed = _map.FindAction("Move", true);
        _moveDelta = _map.FindAction("MoveDelta", true);
        _zoom = _map.FindAction("Zoom", true);
        _cancelPressed = _map.FindAction("Cancel", true);
    }

    public void Subscribe(IPaintingInputReceiver receiver)
    {
        if(_subs.Contains(receiver))
        {
            Debug.LogWarning("Receiver is already subscribed!");
            return;
        }

        _subs.Add(receiver);
    }

    public void Unsubscribe(IPaintingInputReceiver receiver)
    {
        if(!_subs.Contains(receiver))
        {
            Debug.LogWarning("There is no such receivers!");
            return;
        }

        _subs.Remove(receiver);
    }

    public void Enable() => _map.Enable();

    public void Disable() => _map.Disable();

    public void ReadAndDispatch()
    {
        var movePressed = _movePressed.IsPressed();
        var cancelPressed = _cancelPressed.WasPressedThisFrame();
        var moveDelta = _moveDelta.ReadValue<Vector2>();
        var zoom = _zoom.ReadValue<float>();

        var frame = new PaintingInputFrame(movePressed, moveDelta, zoom, cancelPressed);

        for(int i = 0; i < _subs.Count; i++)
        {
            var sub = _subs[i];
            sub.OnInput(frame);
        }
    }
}