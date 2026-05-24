using System;
using UnityEngine;
using Zenject;
using Zenject.SpaceFighter;

public class HeadBobbingService : IInitializable, IDisposable
{
    private readonly SignalBus _bus;

    private PlayerHeadMoveConfig _config;

    private Transform _cameraVisualsTransform;
    private Transform _cameraVisualsParent;
    private Vector3 _initPos;

    private float _term = 0;
    private float _deltaTerm = 0.75f;
    private float _eps = 0.02f;
    private bool _termChanged = false;

    private float _absMaxTerm = 0.5f;

    public HeadBobbingService(SignalBus bus,
                              PlayerHeadMoveConfig config,
                              Transform cameraVisualsTransform)
    {
        _bus = bus;
        _config = config;

        _cameraVisualsTransform = cameraVisualsTransform;
        _cameraVisualsParent = cameraVisualsTransform.parent;
        _initPos = cameraVisualsTransform.localPosition;

        _absMaxTerm = _absMaxTerm < 0f ? -_absMaxTerm : _absMaxTerm;
    }

    public void Initialize()
    {
        _bus.Subscribe<WalkingSignal>(MakeCameraBob);
        _bus.Subscribe<StandingSignal>(ResetCameraBob);
    }

    public void Dispose()
    {
        _bus.Unsubscribe<WalkingSignal>(MakeCameraBob);
        _bus.Unsubscribe<StandingSignal>(ResetCameraBob);
    }

    private void MakeCameraBob(WalkingSignal signal)
    {
        if(!_termChanged && (_term > _absMaxTerm - _eps || _term < -_absMaxTerm + _eps))
        {
            _deltaTerm = -_deltaTerm;
            _termChanged = true;

            StepEmitted stepEmitSignal = new StepEmitted();
            _bus.Fire(stepEmitSignal);
        } 
        else
        {
            StepDone stepDoneSignal = new StepDone();
            _bus.Fire(stepDoneSignal);    
        }
        
        if (Mathf.Abs(_term) < _absMaxTerm - _eps) _termChanged = false;

        _term += _deltaTerm * Time.deltaTime * signal.HorizontalSpeed;
        _term = Mathf.Clamp(_term, -_absMaxTerm, _absMaxTerm);

        Vector3 upLocal = _cameraVisualsParent.InverseTransformDirection(Vector3.up);
        Vector3 visualsRight = _cameraVisualsParent.InverseTransformDirection(_cameraVisualsParent.right);

        float xPos = _term * _config.BobX;
        float yPos = _config.BobCurve.Evaluate(_term) * _config.BobY;
        Vector3 newPos = upLocal * yPos + visualsRight * xPos;

        HeadChangePositionSignal headBobbingSignal = new HeadChangePositionSignal() { NewPosition = newPos, HeadMoveType = HeadMove.HeadBob };
        _bus.Fire(headBobbingSignal);
    }

    private void ResetCameraBob()
    {
        HeadChangePositionSignal headBobbingSignal = new HeadChangePositionSignal() { NewPosition = Vector3.zero, HeadMoveType = HeadMove.HeadBob };
        _bus.Fire(headBobbingSignal);
    }
}
