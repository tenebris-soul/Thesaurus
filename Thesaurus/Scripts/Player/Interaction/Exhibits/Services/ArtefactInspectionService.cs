using System;
using UnityEngine;
using Zenject;

public sealed class ArtefactInspectionService : IArtefactInputReceiver, IInitializable, IDisposable, ITickable, ILateTickable
{
    private readonly SignalBus _bus;
    private readonly IInputRouter _router;

    private readonly Transform _cameraAttach;
    private readonly Camera _camera;
    private readonly ICameraRayProvider _rayProvider;

    private readonly LayerMask _worldMask;
    
    private ArtifactInputFrame _frame;

    private ArtifactObject _artifact;
    
    private float _distance = 0.75f;

    private bool _isInspecting = false;

    private Vector3 _artefactCenter;
    private float _yaw, _pitch;
    private bool _wasRotating = false;  

    private bool _zoomBlocked = false;
    private float _zoomSensitivity = 3f;
    private float _minDist;
    private float _maxDist;

    private float _boundsRadius;
    private float _fitDist;

    private float _minMul = 0.75f;        // насколько близко можно подойти к fitDist
    private float _maxMulSmall = 1.75f;    // zoom-out для мелких объектов
    private float _maxMulLarge = 0.85f;   // zoom-out для больших объектов
    private float _largeRadius = 1.5f;    // начиная с этого радиуса считаем "большой"
    private float _maxExtraUnits = 2.0f;  // доп. лимит "ещё на N метров" (опционально)
    private float _startMul = 1.08f;      // старт чуть дальше fitDist

    private float _framingX = 0.25f; 


    public ArtefactInspectionService(SignalBus bus,
                                     IInputRouter router,
                                     Transform cameraAttach,
                                     Camera camera,
                                     ICameraRayProvider rayProvider,
                                     LayerMask worldMask)
    {
        _bus = bus;
        _router = router;
        _cameraAttach = cameraAttach;
        _camera = camera;
        _rayProvider = rayProvider;
        _worldMask = worldMask;
    }

    public void Initialize()
    {
        _router.Subscribe(InputMode.InspectArtefact, (IArtefactInputReceiver)this);

        _bus.Subscribe<ArtefactModeSignal>(OnArtefactMode);
        _bus.Subscribe<ArtefactScrollPointedSignal>(ToggleZoom);
    }

    public void Dispose()
    {
        _router.Unsubscribe(InputMode.InspectArtefact, (IArtefactInputReceiver)this);

        _bus.Unsubscribe<ArtefactModeSignal>(OnArtefactMode);
        _bus.Unsubscribe<ArtefactScrollPointedSignal>(ToggleZoom);
    }

    public void Tick()
    {
        if(!_isInspecting) return;

        CheckIfCancelPressed();
        ZoomCamera();
        TryReachInterestPoint();
    }

    public void LateTick()
    {
        if(!_isInspecting) return;

        RotateCameraWhenNeeded();
        ApplyOrbit();
    }

    public void OnInput(in ArtifactInputFrame frame)
    {
        _frame = frame;
    }

    private void TryReachInterestPoint()
    {
        if(!_frame.InteractPerformed) return;

        var ray = _rayProvider.GetMouseRay();
        
        if(!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _worldMask, QueryTriggerInteraction.Ignore))
            return;

        int idx = Array.IndexOf(_artifact.InterestPoints, hit.collider);

        if(idx < 0) return;

        _artifact.InterestPoints[idx].gameObject.SetActive(false);

        ArtefactInterestPointFound signal = new(idx);
        _bus.Fire(signal);
    }
    
    private void OnArtefactMode(ArtefactModeSignal s)
    {
        if (!s.IsActive)
        {
            _artifact = null;
            _isInspecting = false;
            return;
        }

        _artifact = s.Artifact;
        _isInspecting = true;

        TranslateAndRotateCamera();
    }

    private void ZoomCamera()
    {
        if(_zoomBlocked) return;

        float z = _frame.Zoom;
        if (Mathf.Abs(z) < 0.0001f) return;

        _distance *= Mathf.Exp(-z * _zoomSensitivity * Time.deltaTime);

        if (_camera != null)
        {
            float clipSafeMin = _boundsRadius + _camera.nearClipPlane + 0.02f;
            _minDist = Mathf.Max(_minDist, clipSafeMin);
        }

        _distance = Mathf.Clamp(_distance, _minDist, _maxDist);
    }

    private void RotateCameraWhenNeeded()
    {
        if (!_frame.RotateHold)
        {
            _wasRotating = false;
            return;
        }

        if (!_wasRotating)
        {
            SyncAnglesFromCurrentCamera();
            _wasRotating = true;
        }

        Vector2 delta = _frame.RotateDelta;

        _yaw   += delta.x * 10f * Time.deltaTime;
        _pitch -= delta.y * 10f * Time.deltaTime;

        _pitch = Mathf.Clamp(_pitch, -40f, 40f);
    }

    private void SyncAnglesFromCurrentCamera()
    {
        Vector3 offset = _cameraAttach.position - _artefactCenter;

        _distance = Mathf.Clamp(offset.magnitude, _minDist, _maxDist);
        if (_distance < 0.0001f) _distance = 0.0001f;

        Vector3 dir = offset / _distance;

        _yaw   = Mathf.Atan2(-dir.x, -dir.z) * Mathf.Rad2Deg;
        _pitch = Mathf.Asin(dir.y) * Mathf.Rad2Deg;
    }

    private void ApplyOrbit()
    {
        Quaternion rot = Quaternion.Euler(_pitch, _yaw, 0f);

        Vector3 camPos = _artefactCenter + rot * (Vector3.back * _distance);

        float lookOffset = 0f;
        if (_camera != null)
        {
            float vFov = _camera.fieldOfView * Mathf.Deg2Rad;
            float hFov = 2f * Mathf.Atan(Mathf.Tan(vFov * 0.5f) * _camera.aspect);

            lookOffset = _framingX * _distance * Mathf.Tan(hFov * 0.5f);
        }
        else
        {
            lookOffset = _framingX * _distance;
        }

        Vector3 right = rot * Vector3.right;
        Vector3 lookTarget = _artefactCenter + right * lookOffset;

        _cameraAttach.position = camPos;
        _cameraAttach.LookAt(lookTarget);
    }

    private void CheckIfCancelPressed()
    {
        if (!_frame.CancelPressed) return;

        _frame = default;

        _bus.Fire(new ArtefactModeSignal { IsActive = false, Artifact = null });
        _bus.Fire(new SearchModeSignal { IsActive = true });
    }

    private void TranslateAndRotateCamera()
    {
        if(!_isInspecting) return;

        var cols = _artifact.Colliders;
        if(cols == null || cols.Length == 0) return;

        Bounds b = (cols.Length == 1) ? cols[0].bounds : GetArtifactCombinedBounds(cols);

        _artefactCenter = b.center;

        ConfigureDistancesFromBounds(b);

        Vector3 dir = (_cameraAttach.position - _artefactCenter).normalized;
        if (dir.sqrMagnitude < 0.0001f) dir = Vector3.back;

        _cameraAttach.position = _artefactCenter + dir * _distance;
        _cameraAttach.LookAt(_artefactCenter);

        SyncAnglesFromCurrentCamera();
        _wasRotating = false;
    }


    private Bounds GetArtifactCombinedBounds(Collider[] cols)
    {
        bool inited = false;
        Bounds b = default;

        for(int i = 0; i < cols.Length; i++)
        {
            if(!inited)
            {
                b = cols[i].bounds;
                inited = true;
            }
            else
            {
                b.Encapsulate(cols[i].bounds);
            }
        }

        if(!inited) b = new Bounds(Vector3.zero, Vector3.zero);
        return b;
    }

    private void ConfigureDistancesFromBounds(Bounds b)
    {
        float r = b.extents.magnitude;
        r = Mathf.Max(r, 0.01f);

        _boundsRadius = r;

        const float padding = 1.15f;

        if (_camera == null)
        {
            _fitDist = r * 3f * padding;
            _minDist = Mathf.Max(r * 1.2f, 0.05f);
            _maxDist = _fitDist * 2.5f;
            _distance = Mathf.Clamp(_fitDist, _minDist, _maxDist);
            return;
        }

        float vFov = _camera.fieldOfView * Mathf.Deg2Rad;
        float hFov = 2f * Mathf.Atan(Mathf.Tan(vFov * 0.5f) * _camera.aspect);

        float half = Mathf.Min(vFov, hFov) * 0.5f;

        _fitDist = r / Mathf.Sin(half) * padding;

        float clipSafeMin = r + _camera.nearClipPlane + 0.02f;

        float t = Mathf.InverseLerp(0.2f, _largeRadius, r);        
        float maxMul = Mathf.Lerp(_maxMulSmall, _maxMulLarge, t);

        _minDist = Mathf.Max(_fitDist * _minMul, clipSafeMin);

        _maxDist = _fitDist * maxMul;

        _maxDist = Mathf.Min(_maxDist, _fitDist + _maxExtraUnits);

        _distance = Mathf.Clamp(_fitDist * _startMul, _minDist, _maxDist);
    }

    private void ToggleZoom(ArtefactScrollPointedSignal signal)
    {
        _zoomBlocked = signal.IsPointing;
    }
}