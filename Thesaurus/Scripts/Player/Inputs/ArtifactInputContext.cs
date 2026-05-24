using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class ArtifactInputContext : IInputContext<IArtefactInputReceiver>
{
    public string ActionMap => "InspectArtifact";
    private readonly InputActionMap _map;
    private readonly List<IArtefactInputReceiver> _subs = new();

    private readonly InputAction _interactAndRotate;
    private readonly InputAction _rotateDelta;
    private readonly InputAction _zoom;
    private readonly InputAction _cancel;

    public ArtifactInputContext(InputActionAsset asset
                                // List<IArtifactInputReceiver> subs
                                )
    {
        _map = asset.FindActionMap(ActionMap, true);

        _interactAndRotate = _map.FindAction("InteractAndRotate", true);
        _rotateDelta = _map.FindAction("RotateDelta", true);
        _zoom = _map.FindAction("Zoom", true);
        _cancel = _map.FindAction("Cancel", true);

        // _subs = subs;
    }

    public void Disable() => _map.Disable();
    public void Enable() => _map.Enable();

    public void ReadAndDispatch()
    {
        var rotateHold = _interactAndRotate.IsPressed();
        var interactPerformed = _interactAndRotate.WasPressedThisFrame();
        var rotateDelta = _rotateDelta.ReadValue<Vector2>();
        var zoomInPressed = _zoom.ReadValue<float>();
        var cancelPressed = _cancel.WasPressedThisFrame();

        var frame = new ArtifactInputFrame(rotateHold, interactPerformed, rotateDelta, zoomInPressed, cancelPressed);

        for(int i = 0; i < _subs.Count; i++)
        {
            var sub = _subs[i];
            sub.OnInput(frame);
        }
    }

    public void Subscribe(IArtefactInputReceiver receiver)
    {
        if(_subs.Contains(receiver))
        {
            Debug.LogWarning("Receiver is already subscribed!");
            return;
        }

        _subs.Add(receiver);
    }

    public void Unsubscribe(IArtefactInputReceiver receiver)
    {
        if(!_subs.Contains(receiver))
        {
            Debug.LogWarning("There is no such receivers!");
            return;
        }

        _subs.Remove(receiver);
    }
}