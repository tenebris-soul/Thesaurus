using System;
using UnityEngine;
using Zenject;

public sealed class ModeCoordinator : IInitializable, IDisposable
{
    private readonly SignalBus _bus;
    private readonly IInputRouter _router;

    private IDisposable _lease;

    public ModeCoordinator(SignalBus bus, IInputRouter router)
    {
        _bus = bus;
        _router = router;
    }

    public void Initialize()
    {
        _bus.Subscribe<InspectModeSignal>(OnExhibitInspect);
        _bus.Subscribe<ArtefactModeSignal>(OnArtifactInspect);
        _bus.Subscribe<PaintingModeSignal>(OnPaintingInspect);
    }

    public void Dispose()
    {
        _bus.Unsubscribe<InspectModeSignal>(OnExhibitInspect);
        _bus.Unsubscribe<ArtefactModeSignal>(OnArtifactInspect);
        _bus.Unsubscribe<PaintingModeSignal>(OnPaintingInspect);
        
        StopExhibitInspect();
        StopArtifactInspect();
        StopPaintingInspect();
    }

    // inspect mode
    private void OnExhibitInspect(InspectModeSignal signal)
    {
        if(signal.IsActive) StartExhibitInspect();
        else StopExhibitInspect();
    }

    private void StartExhibitInspect()
    {
        if(_lease != null) return;

        _lease = _router.PushMode(InputMode.InspectExhibit, this);
    }

    private void StopExhibitInspect()
    {
        if(_lease == null) return;

        _lease.Dispose();
        _lease = null;
    }

    // artifact mode
    private void OnArtifactInspect(ArtefactModeSignal signal)
    {
        if(signal.IsActive) StartArtifactInspect();
        else StopArtifactInspect();
    }

    private void StartArtifactInspect()
    {
        if(_lease != null) return;

        _lease = _router.PushMode(InputMode.InspectArtefact, this);
    }

    private void StopArtifactInspect()
    {
        if(_lease == null) return;

        _lease.Dispose();
        _lease = null;
    }

    // painting mode
    private void OnPaintingInspect(PaintingModeSignal signal)
    {
        if(signal.IsActive) StartPaintingInspect();
        else StopPaintingInspect();
    }

    private void StartPaintingInspect()
    {
        if(_lease != null) return;

        _lease = _router.PushMode(InputMode.InspectPainting, this);
    }

    private void StopPaintingInspect()
    {
        if(_lease == null) return;

        _lease.Dispose();
        _lease = null;
    }
}