using UnityEngine;

public class CeilingCheckService : ICeilingCheckService
{
    private readonly CapsuleCollider _playerCollider;
    private readonly LayerMask _worldMask;

    private readonly ICapsuleGeometryService _capsuleGeometryService;

    private readonly PlayerMovementTuning _playerMovementTuning;

    private readonly PlayerWriteContext _playerWriteContext;

    public CeilingCheckService(CapsuleCollider playerCollider,
                               LayerMask worldMask,
                               ICapsuleGeometryService capsuleGeometryService,
                               PlayerMovementTuning playerMovementTuning,
                               PlayerWriteContext playerWriteContext)
    {
        _playerCollider = playerCollider;
        _worldMask = worldMask;
        _capsuleGeometryService = capsuleGeometryService;
        _playerMovementTuning = playerMovementTuning;
        _playerWriteContext = playerWriteContext;
    }

    public void ProbeCeiling(Vector3 playerPosition)
    {
        _capsuleGeometryService.GetCapsulePoints(playerPosition, out Vector3 p1, out _);

        float radius = _playerCollider.radius;
        float dist = _playerMovementTuning.PROBE_UP;
        var QTI = _playerMovementTuning.QTI;

        if(!Physics.SphereCast(p1, radius, Vector3.up,
                               out _, dist, _worldMask, 
                               QTI))
        {
            _playerWriteContext.SetCeilingHitting(false);
            return;
        }

        _playerWriteContext.SetCeilingHitting(true);
    }
}
