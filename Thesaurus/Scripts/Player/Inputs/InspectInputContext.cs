using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class InspectInputContext : IInputContext<IInspectInputReceiver>
{
    public string ActionMap => "InspectExhibit";
    private readonly InputActionMap _map;
    private readonly List<IInspectInputReceiver> _subs = new();

    private readonly InputAction _cancel;

    private const string CANCEL_ACTION = "Cancel";

    public InspectInputContext(InputActionAsset asset
                                // List<IInspectInputReceiver> subs
                                )
    {
        _map = asset.FindActionMap(ActionMap, true);

        _cancel = _map.FindAction(CANCEL_ACTION, true);;

        // _subs = subs;
    }

    public void Disable() => _map.Disable();

    public void Enable() => _map.Enable();

    public void ReadAndDispatch()
    {
        var cancel = _cancel.WasPressedThisFrame();

        var frame = new InspectInputFrame(cancel);

        for(int i = 0; i < _subs.Count; i++)
        {
            var sub = _subs[i];
            sub.OnInput(frame);
        }
    }

    public void Subscribe(IInspectInputReceiver receiver)
    {
        if(_subs.Contains(receiver))
        {
            Debug.LogWarning("Receiver is already subscribed!");
            return;
        }

        _subs.Add(receiver);
    }

    public void Unsubscribe(IInspectInputReceiver receiver)
    {
        if(!_subs.Contains(receiver))
        {
            Debug.LogWarning("There is no such receivers!");
            return;
        }

        _subs.Remove(receiver);
    }
}