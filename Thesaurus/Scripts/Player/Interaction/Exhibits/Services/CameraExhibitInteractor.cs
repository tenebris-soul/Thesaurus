using System;
using UnityEngine;
using Zenject;

public sealed class CameraExhibitInteractor : ITickable, IGameplayInputReceiver, IInitializable, IDisposable
{
    private readonly ICameraRayProvider _rayProvider;
    private readonly SignalBus _bus;
    private readonly IInputRouter _router;
    
    private GameplayInputFrame _frame;
    
    private ExhibitObject _currentExhibit;
    private ArtifactObject _currentArtifact;
    private PaintingObject _currentPainting;

    private const float DISTANCE = 1.75f;

    private bool _enabled = false;

    public CameraExhibitInteractor(ICameraRayProvider rayProvider,
                                   SignalBus bus,
                                   IInputRouter router)
    {
        _rayProvider = rayProvider;
        _bus = bus;
        _router = router;
    }

    public void Initialize()
    {
        _bus.Subscribe<SearchModeSignal>(SetActiveSearchMode);
        SetActiveSelf(true);
    }

    public void Dispose()
    {
        _bus.Unsubscribe<SearchModeSignal>(SetActiveSearchMode);
        SetActiveSelf(false);
    }

    public void Tick()
    {
        if(!_enabled) return;

        TrySearchExhibit();
        TryInspectExhibit();

        TrySearchArtifact();
        TryInspectArtifact();

        TrySearchPainting();
        TryInspectPainting();
    }

    public void OnInput(in GameplayInputFrame frame)
    {
        _frame = frame;
    }

    private void SetActiveSearchMode(SearchModeSignal signal)
    {
        SetActiveSelf(signal.IsActive);
    }

    private void SetActiveSelf(bool isActive)
    {
        if (_enabled == isActive) return;
        _enabled = isActive;

        if (_enabled)
        {
            _router.Subscribe(InputMode.Gameplay, (IGameplayInputReceiver)this);
        }
        else
        {
            _router.Unsubscribe(InputMode.Gameplay, (IGameplayInputReceiver)this);

            _frame = default;

            if (_currentExhibit != null)
            {
                _currentExhibit = null;
                _bus.Fire(new InteractableLostSignal());
            } 
            else if (_currentArtifact != null)
            {
                _currentArtifact = null;
                _bus.Fire(new InteractableLostSignal());
            }
            else if (_currentPainting != null)
            {
                _currentPainting = null;
                _bus.Fire(new InteractableLostSignal());
            }
        }
    }

    // exhibit inspect
    private void TrySearchExhibit()
    {
        var ray = _rayProvider.GetForwardRay();

        ExhibitObject newTarget = null;

        if(Physics.Raycast(ray, out RaycastHit hitInfo, DISTANCE))
        {
            if(ExhibitsRegistry.TryGetExhibit(hitInfo.collider, out ExhibitObject exhibit))
                newTarget = exhibit;
        }

        if(!ReferenceEquals(newTarget, _currentExhibit))
        {
            if(_currentExhibit != null)
                _bus.Fire(new InteractableLostSignal());

            _currentExhibit = newTarget;

            if(_currentExhibit != null)
            {
                InteractableFoundSignal signal = new InteractableFoundSignal
                {
                    Type = InteractableTypes.Exhibit
                };
                _bus.Fire(signal);
            }
        }
    }

    private void TryInspectExhibit()
    {
        if(_currentExhibit == null) return;
        if(!_frame.InteractPressed) return;
        
        InspectModeSignal inspectSignal = new()
        {
            Exhibit = _currentExhibit,
            IsActive = true
        };
        SearchModeSignal searchSignal = new()
        {
            IsActive = false
        };

        _bus.Fire(inspectSignal);
        _bus.Fire(searchSignal);

        SetActiveSelf(false);
    }

    // artifact inspect
    private void TrySearchArtifact()
    {
        var ray = _rayProvider.GetForwardRay();

        ArtifactObject newTarget = null;

        if(Physics.Raycast(ray, out RaycastHit hitInfo, DISTANCE))
        {
            if(ArtifactRegistry.TryGetArtifact(hitInfo.collider, out ArtifactObject artifact))
                newTarget = artifact;
        }

        if(!ReferenceEquals(newTarget, _currentArtifact))
        {
            if(_currentArtifact != null)
                _bus.Fire(new InteractableLostSignal());

            _currentArtifact = newTarget;

            if(_currentArtifact != null)
            {
                InteractableFoundSignal signal = new InteractableFoundSignal
                {
                    Type = InteractableTypes.Artifact
                };
                _bus.Fire(signal);
            }
        }
    }

    private void TryInspectArtifact()
    {
        if(_currentArtifact == null) return;
        if(!_frame.InteractPressed) return;

        ArtefactModeSignal inspectSignal = new()
        {
            Artifact = _currentArtifact,
            IsActive = true
        };
        SearchModeSignal searchSignal = new()
        {
            IsActive = false
        };

        _bus.Fire(inspectSignal);
        _bus.Fire(searchSignal);

        SetActiveSelf(false);
    }

    // painting mode
    private void TrySearchPainting()
    {
        var ray = _rayProvider.GetForwardRay();

        PaintingObject newTarget = null;

        if(Physics.Raycast(ray, out RaycastHit hitInfo, DISTANCE))
        {
            if(PaintingsRegistry.TryGetPainting(hitInfo.collider, out PaintingObject painting))
                newTarget = painting;
        }

        if(!ReferenceEquals(newTarget, _currentPainting))
        {
            if(_currentPainting != null)
                _bus.Fire(new InteractableLostSignal());

            _currentPainting = newTarget;

            if(_currentPainting != null)
            {
                InteractableFoundSignal signal = new InteractableFoundSignal
                {
                    Type = InteractableTypes.Painting
                };
                _bus.Fire(signal);
            }
        }
    }

    private void TryInspectPainting()
    {
        if(_currentPainting == null) return;
        if(!_frame.InteractPressed) return;

        PaintingModeSignal inspectSignal = new()
        {
            Painting = _currentPainting,
            IsActive = true
        };
        SearchModeSignal searchSignal = new()
        {
            IsActive = false
        };

        _bus.Fire(inspectSignal);
        _bus.Fire(searchSignal);

        SetActiveSelf(false);
    }
}