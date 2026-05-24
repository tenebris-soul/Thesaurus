using System;
using UnityEngine;
using Zenject;

public class HeadMoverService : IHeadMoverService, ITickable
{   
    private Transform _cameraVisualsTransform;

    private Vector3 _initPos;

    private Vector3 _headBobOffset;
    private Vector3 _landingOffset;

    private float _k = 5f;

    public HeadMoverService(Transform cameraVisualsTransform)
    {
        _cameraVisualsTransform = cameraVisualsTransform;

        _initPos = _cameraVisualsTransform.localPosition;
    }

    public void SetTargetPosition(HeadMove headMove, Vector3 pos)
    {
        switch(headMove)
        {
            case HeadMove.HeadBob:
                _headBobOffset = pos;
                break;
            case HeadMove.Landing:
                _landingOffset = pos;
                break;
            default:
                break;
        }
    }

    public void Tick()
    {
        float t = 1f - Mathf.Exp(-_k * Time.deltaTime);

        Vector3 currentPos = _cameraVisualsTransform.localPosition;
        Vector3 targetPos = _initPos + _headBobOffset + _landingOffset;

        Vector3 nextPos = Vector3.Lerp(currentPos, targetPos, t);

        _cameraVisualsTransform.localPosition = nextPos;
    }
}
