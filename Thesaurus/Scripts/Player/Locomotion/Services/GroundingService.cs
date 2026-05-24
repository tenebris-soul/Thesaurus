using System;
using UnityEngine;

public class GroundingService : IGroundingService
{
    private readonly CapsuleCollider _playerCollider;
    private readonly LayerMask _worldMask;

    private readonly ISlopeChecker _slopeChecker;
    private readonly ICapsuleGeometryService _capsuleGeometryService;
    private readonly PlayerMovementTuning _playerMovementTuning;

    private readonly PlayerWriteContext _playerWriteContext;
    private readonly PlayerReadContext _PlayerReadMotorContext;

    private float _bestHitDistance;

    private float _tolerance;
    private float _tinyTolerance;

    public GroundingService(
        CapsuleCollider playerCollider,
        LayerMask worldMask,
        ISlopeChecker slopeChecker,
        ICapsuleGeometryService capsuleGeometryService,
        PlayerMovementTuning playerMovementTuning,
        PlayerWriteContext playerWriteContext,
        PlayerReadContext PlayerReadMotorContext)
    {
        _playerCollider = playerCollider;
        _worldMask = worldMask;
        _slopeChecker = slopeChecker;
        _capsuleGeometryService = capsuleGeometryService;
        _playerMovementTuning = playerMovementTuning;
        _playerWriteContext = playerWriteContext;
        _PlayerReadMotorContext = PlayerReadMotorContext;

        _tolerance = 0.1f * _playerCollider.radius;
        _tinyTolerance = 0.2f * _tolerance;
    }

    public void ProbeGround(Vector3 playerPosition, out Rigidbody groundRb)
    {
        ProbeSetup setup = BuildProbeSetup(playerPosition);

        if(!Physics.SphereCast(setup.bottomSpherePos + setup.up * _tolerance, _playerCollider.radius, 
                               setup.down, out RaycastHit hitInfo, _playerMovementTuning.PROBE_DOWN,
                               _worldMask, _playerMovementTuning.QTI))
        {
            ClearGround(setup.up, out groundRb);
            return;
        }

        ApplyGround(hitInfo, out groundRb);
    }

    public struct ProbeSetup
    {
        public Vector3 up;
        public Vector3 down;
        public Vector3 bottomSpherePos;
    }

    public ProbeSetup BuildProbeSetup(Vector3 playerPosition)
    {
        Vector3 up = _playerCollider.transform.up;
        Vector3 down = -up;

        _capsuleGeometryService.GetCapsulePoints(playerPosition, out _, out Vector3 bottom);

        return new ProbeSetup
        {
            up = up,
            down = down,
            bottomSpherePos = bottom
        };
    }

    private void ClearGround(Vector3 up, out Rigidbody groundRb)
    {
        _playerWriteContext.SetGroundedState(false);
        _playerWriteContext.SetSteepGround(false);
        _playerWriteContext.SetGroundNormal(up);
        _playerWriteContext.SetGroundCollider(null);
        _bestHitDistance = 0f;
        groundRb = null;
    }

    private void ApplyGround(RaycastHit appliedHit, out Rigidbody groundRb)
    {
        _playerWriteContext.SetGroundedState(true);
        _playerWriteContext.SetSteepGround(!_slopeChecker.CheckSlopeAngle(appliedHit.normal));
        _playerWriteContext.SetGroundNormal(appliedHit.normal);
        _playerWriteContext.SetGroundCollider(appliedHit.collider);
        _bestHitDistance = appliedHit.distance;
        groundRb = appliedHit.rigidbody ? appliedHit.rigidbody : appliedHit.collider.attachedRigidbody;
    }

    public Vector3 SoftSnapToGround(Vector3 position, Vector3 velocity)
    {
        if (!_PlayerReadMotorContext.IsGrounded)
            return position;

        Vector3 gravityDir = -_playerCollider.transform.up;

        Vector3 groundNormal = _PlayerReadMotorContext.GroundNormal;

        const float detachEps = 0.001f;
        if (Vector3.Dot(velocity, groundNormal) > detachEps)
            return position;

        float clearance = _playerMovementTuning.PROBE_UP_EXTENT;

        float snapDistance = _bestHitDistance - (clearance + _tolerance);

        const float snapEps = 0.002f;
        if (snapDistance <= snapEps)
            return position;

        snapDistance = Mathf.Min(snapDistance, _playerMovementTuning.PROBE_DOWN);

        position += gravityDir * snapDistance;

        return position;
    }
}
