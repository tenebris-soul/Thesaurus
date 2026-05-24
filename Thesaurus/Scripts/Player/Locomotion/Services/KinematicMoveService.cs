using UnityEngine;

public class KinematicMoveService : IKinematicMoveService
{
    private readonly CapsuleCollider _playerCollider;
    private readonly LayerMask _worldMask;

    private readonly ICapsuleGeometryService _capsuleGeometryService;
    private readonly ISlopeChecker _slopeChecker;
    private readonly IStepUpService _stepUpService;

    private readonly PlayerMovementTuning _playerMovementTuning;

    private float _debugUntilTime;

    public KinematicMoveService(CapsuleCollider playerCollider, 
                                LayerMask worldMask, 
                                ICapsuleGeometryService capsuleGeometryService,
                                ISlopeChecker slopeChecker,
                                IStepUpService stepUpService,
                                PlayerMovementTuning playerMovementTuning
                                )
    {
        _playerCollider = playerCollider;
        _worldMask = worldMask;
        _capsuleGeometryService = capsuleGeometryService;
        _slopeChecker = slopeChecker;
        _stepUpService = stepUpService;
        _playerMovementTuning = playerMovementTuning;
    }

    public Vector3 MoveWithCollisions(Vector3 playerPosition,
                                      Vector3 delta
                                      )
    {
        int moveMaxIter = _playerMovementTuning.MOVE_MAX_ITER;
        float skinWidth = _playerMovementTuning.SKIN_WIDTH;
        var QTI = _playerMovementTuning.QTI;

        Vector3 remaining = delta;

        for (int i = 0; i < moveMaxIter; i++)
        {
            float remainDist = remaining.magnitude;
            if (remainDist < 1e-3f) break;

            Vector3 remainDir = (remaining / remainDist).normalized;

            _capsuleGeometryService.GetCapsulePoints(playerPosition, out Vector3 p1, out Vector3 p2);

            if (!Physics.CapsuleCast(p1, p2, _playerCollider.radius,
                                    remainDir, out RaycastHit hit,
                                    remainDist + skinWidth,
                                    _worldMask, QTI))
            {
                playerPosition += remaining;
                break;
            }

            float travel = Mathf.Max(0f, hit.distance - skinWidth);
            playerPosition += remainDir * travel;

            Vector3 left = remaining - remainDir * travel;

            float leftDist = left.magnitude;

            if (leftDist < 1e-3f)
                break;

            Vector3 leftDir = (left / leftDist).normalized;

            bool isWalkable = _slopeChecker.CheckSlopeAngle(hit.normal);

            Vector3 slide;
            if (isWalkable)
            {
                slide = Vector3.ProjectOnPlane(left, hit.normal);
                //Debug.Log($"{hit.normal} {hit.point} {left}");
            }
            else
            {
                if (_stepUpService.TryStepUp(ref playerPosition,
                                            leftDir, 
                                            leftDist))
                {
                   return playerPosition;
                }

                Vector3 wallN = Vector3.ProjectOnPlane(hit.normal, Vector3.up);
                if (wallN.sqrMagnitude < 1e-6f)
                    return playerPosition;

                slide = Vector3.ProjectOnPlane(leftDir, wallN.normalized) * leftDist;
            }

            float slideDist = slide.magnitude;
            if (slideDist < 1e-3f)
                break;

            remaining = slide;
            continue;
        }

        return playerPosition;
    }
}
