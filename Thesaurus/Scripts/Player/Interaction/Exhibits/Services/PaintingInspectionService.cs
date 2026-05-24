using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public sealed class PaintingInspectionService : IPaintingInputReceiver, IInitializable, IDisposable, ITickable
{
    private readonly SignalBus _bus;
    private readonly IInputRouter _router;

    private readonly Camera _camera;
    private readonly Transform _cameraAttach;

    private PaintingInputFrame _frame;

    private PaintingObject _painting;

    private bool _isInspecting = false;

    private Vector3 _panOffset;

    private float _defaultDist;
    private float _distance = 1.5f;
    private float _zoomSensitivity = 0.15f;
    private float _minDist;
    private float _maxDist;

    private const float FitPadding = 1.10f;   


    public PaintingInspectionService(SignalBus bus,
                                     IInputRouter router,
                                     Transform cameraAttach,
                                     Camera camera)
    {
        _bus = bus;
        _router = router;
        _cameraAttach = cameraAttach;
        _camera = camera;
    }

    public void Initialize()
    {
        _router.Subscribe<IPaintingInputReceiver>(InputMode.InspectPainting, this);

        _bus.Subscribe<PaintingModeSignal>(OnPaintingMode);
    }

    public void Dispose()
    {
        _router.Unsubscribe(InputMode.InspectPainting, (IPaintingInputReceiver)this);

        _bus.Unsubscribe<PaintingModeSignal>(OnPaintingMode);
    }

    public void Tick()
    {
        MoveCameraWhenNeeded();
        ZoomCamera();
        CheckIfCancelPressed();
    }

    public void OnInput(in PaintingInputFrame frame) => _frame = frame;

    private void OnPaintingMode(PaintingModeSignal signal)
    {
        if(!signal.IsActive)
        {
            _isInspecting = false;
            _painting = null;
            return;
        }

        _painting = signal.Painting;

        if (_painting == null)
        {
            Debug.LogWarning("PaintingModeSignal active, but Painting is null");
            return;
        }

        _isInspecting = true;
        _panOffset = Vector3.zero;

        RecalculateZoomBounds(); 

        _distance = _defaultDist;
        TranslateAndRotateCamera();
    }

    private void RecalculateZoomBounds()
    {
        GetPaintingHalfExtents(out float halfW, out float halfH);

        float fitDist = CalculateFitDistancePerspective(halfW, halfH);

        _maxDist = fitDist * FitPadding; 

        float nearSafe = (_camera != null ? _camera.nearClipPlane : 0.01f) + 0.02f;
        _minDist = Mathf.Max(nearSafe, _maxDist * 0.10f); 

        if (_maxDist < _minDist) _maxDist = _minDist + 0.01f;

        _defaultDist = _maxDist;

        _distance = Mathf.Clamp(_distance, _minDist, _maxDist);
    }


    private float CalculateFitDistancePerspective(float halfW, float halfH)
    {
        if (_camera == null) return 3f; 

        float vFovRad = _camera.fieldOfView * Mathf.Deg2Rad;
        float tanV = Mathf.Tan(vFovRad * 0.5f);

        float tanH = tanV * _camera.aspect;

        float distByHeight = halfH / tanV;
        float distByWidth  = halfW / tanH;

        return Mathf.Max(distByHeight, distByWidth);
    }

    private void TranslateAndRotateCamera()
    {
        if(_painting == null) return;

        Vector3 paintingForward = _painting.transform.forward;

        paintingForward = _painting.Axis switch
        {
            PaintingAxis.X => _painting.transform.right,
            PaintingAxis.NegativeX => -_painting.transform.right,
            PaintingAxis.Y => _painting.transform.up,
            PaintingAxis.NegativeY => -_painting.transform.up,
            PaintingAxis.Z => _painting.transform.forward,
            PaintingAxis.NegativeZ => -_painting.transform.forward,
            _ => paintingForward
        };

        _cameraAttach.position = _painting.transform.position - paintingForward * _distance;
        _cameraAttach.LookAt(_painting.transform.position);
    }

    private void ZoomCamera()
    {
        if (!_isInspecting || _painting == null) return;

        float z = _frame.Zoom;
        if (Mathf.Abs(z) < 0.0001f) return;

        _distance *= Mathf.Exp(-z * _zoomSensitivity);

        _distance = Mathf.Clamp(_distance, _minDist, _maxDist);

        GetInspectionBasis(out Vector3 forward, out _, out Vector3 up);

        Vector3 target = _painting.transform.position + _panOffset;

        _cameraAttach.position = target - forward * _distance;
    }

    private void MoveCameraWhenNeeded()
    {
        if(!_isInspecting || _painting == null) return;

        if(!_frame.MovePressed) return;

        Vector2 delta = _frame.MoveDelta;
        if(delta.sqrMagnitude < 1e-4f) return;

        GetInspectionBasis(out Vector3 forward, out Vector3 right, out Vector3 up);

        _panOffset += (right * -delta.x + up * -delta.y) * 0.15f * Time.deltaTime;

        ClampPanOffset(right, up);

        Vector3 targetPosition = _painting.transform.position + _panOffset;

        _cameraAttach.position = targetPosition - forward * _distance;
    }

    private void ClampPanOffset(Vector3 right, Vector3 up)
    {
        GetPaintingHalfExtents(out float halfW, out float halfH);

        const float edgePadding = 0.03f;

        float maxX = Mathf.Max(0f, halfW - edgePadding);
        float maxY = Mathf.Max(0f, halfH - edgePadding);

        float x = Vector3.Dot(_panOffset, right);
        float y = Vector3.Dot(_panOffset, up);

        x = Mathf.Clamp(x, -maxX, maxX);
        y = Mathf.Clamp(y, -maxY, maxY);

        _panOffset = right * x + up * y;
    }

    private void CheckIfCancelPressed()
    {
        if(!_isInspecting) return;
        if(!_frame.CancelPressed) return;

        _frame = default;

        PaintingModeSignal paintingSignal = new() { Painting = null, IsActive = false };
        SearchModeSignal searchSignal = new() { IsActive = true };
        
        _bus.Fire(paintingSignal);
        _bus.Fire(searchSignal);
    }

    private void GetInspectionBasis(out Vector3 forward, out Vector3 planeRight, out Vector3 planeUp)
    {
        forward = _painting.Axis switch
        {
            PaintingAxis.X         => _painting.transform.right,
            PaintingAxis.NegativeX => -_painting.transform.right,
            PaintingAxis.Y         => _painting.transform.up,
            PaintingAxis.NegativeY => -_painting.transform.up,
            PaintingAxis.Z         => _painting.transform.forward,
            PaintingAxis.NegativeZ => -_painting.transform.forward,
            _ => _painting.transform.forward
        };

        forward.Normalize();

        Vector3 upCandidate = Mathf.Abs(Vector3.Dot(forward, Vector3.up)) > 0.95f
            ? _painting.transform.forward
            : Vector3.up;

        planeRight = Vector3.Cross(upCandidate, forward).normalized;
        planeUp    = Vector3.Cross(forward, planeRight).normalized;
    }

    private void GetPaintingHalfExtents(out float halfW, out float halfH)
    {
        if (_painting.TryGetComponent<BoxCollider>(out var box))
        {
            Vector3 size = Vector3.Scale(box.size, Abs(_painting.transform.lossyScale));
            SelectWidthHeightFromNormalAxis(size, out halfW, out halfH);
            halfW *= 0.5f;
            halfH *= 0.5f;
            return;
        }

        if (_painting.TryGetComponent<Renderer>(out var r))
        {
            Vector3 size = Vector3.Scale(r.localBounds.size, Abs(_painting.transform.lossyScale));
            SelectWidthHeightFromNormalAxis(size, out halfW, out halfH);
            halfW *= 0.5f;
            halfH *= 0.5f;
            return;
        }

        halfW = halfH = 0.5f;
    }

    private Vector3 Abs(Vector3 v) =>
        new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));

    private void SelectWidthHeightFromNormalAxis(Vector3 localSizeScaled, out float width, out float height)
    {
        switch (_painting.Axis)
        {
            case PaintingAxis.Z:
            case PaintingAxis.NegativeZ:
                width  = localSizeScaled.x;
                height = localSizeScaled.y;
                break;

            case PaintingAxis.X:
            case PaintingAxis.NegativeX:
                width  = localSizeScaled.z;
                height = localSizeScaled.y;
                break;

            case PaintingAxis.Y:
            case PaintingAxis.NegativeY:
                width  = localSizeScaled.x;
                height = localSizeScaled.z;
                break;

            default:
                width  = localSizeScaled.x;
                height = localSizeScaled.y;
                break;
        }
    }
}