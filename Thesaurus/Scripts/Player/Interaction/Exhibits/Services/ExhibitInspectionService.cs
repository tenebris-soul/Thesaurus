using System;
using UnityEngine;
using Zenject;

public sealed class ExhibitInspectionService : IInspectInputReceiver, IInitializable, IDisposable, ITickable
{
    private readonly SignalBus _bus;
    private readonly IInputRouter _router;

    private readonly Transform _cameraAttach;

    private InspectInputFrame _frame;

    private ExhibitObject _exhibit;
    private Transform _anchor;
    private Transform _target;

    private bool _isInspecting = false;

    public ExhibitInspectionService(SignalBus bus,
                                    IInputRouter router,
                                    Transform cameraAttach)
    {
        _bus = bus;
        _router = router;
        _cameraAttach = cameraAttach;
    }

    public void Initialize()
    {
        _router.Subscribe(InputMode.InspectExhibit, (IInspectInputReceiver)this);

        _bus.Subscribe<InspectModeSignal>(OnInspectMode);
    }

    public void Dispose()
    {
        _router.Unsubscribe(InputMode.InspectExhibit, (IInspectInputReceiver)this);

        _bus.Unsubscribe<InspectModeSignal>(OnInspectMode);
    }

    public void Tick()
    {
        CheckIfCancelPressed();
    }

    public void OnInput(in InspectInputFrame frame)
    {
        _frame = frame;
    }

    private void OnInspectMode(InspectModeSignal s)
    {
        if (!s.IsActive)
        {
            _exhibit = null;
            _isInspecting = false;
            return;
        }

        _exhibit = s.Exhibit;

        if (_exhibit == null)
        {
            Debug.LogWarning("InspectModeSignal active, but Exhibit is null");
            return;
        }

        _anchor = _exhibit.CameraAnchor;
        _target = _exhibit.CameraTarget;

        _isInspecting = true;

        TranslateAndRotateCamera();
    }

    private void TranslateAndRotateCamera()
    {
        if(!_isInspecting) return;

        _cameraAttach.position = _anchor.position;
        _cameraAttach.LookAt(_target);
    }

    private void CheckIfCancelPressed()
    {
        if(!_isInspecting) return;
        if(!_frame.CancelPressed) return;

        _frame = default;

        InspectModeSignal inspectSignal = new() { Exhibit = null, IsActive = false };
        SearchModeSignal searchSignal = new() { IsActive = true };
        
        _bus.Fire(inspectSignal);
        _bus.Fire(searchSignal);
    }
}
