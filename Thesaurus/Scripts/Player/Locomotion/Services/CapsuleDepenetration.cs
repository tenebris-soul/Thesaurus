using UnityEngine;

public class CapsuleDepenetration : ICapsuleDepenetration
{
    private readonly CapsuleCollider _playerCollider;
    private readonly LayerMask _worldMask;

    private ICapsuleGeometryService _capsuleGeometryService;

    private readonly PlayerMovementTuning _playerMovementTuning;

    private const int INIT_OVERLAP_BUFFER_SIZE = 16;
    private const int MAX_OVERLAP_BUFFER_SIZE = 256;

    private Collider[] _overlapBuffer;

    public CapsuleDepenetration(CapsuleCollider playerCollider,
                           LayerMask worldMask,
                           ICapsuleGeometryService capsuleGeometryService,
                           PlayerMovementTuning playerMovementTuning)
    {
        _playerCollider = playerCollider;
        _worldMask = worldMask;
        _capsuleGeometryService = capsuleGeometryService;
        _playerMovementTuning = playerMovementTuning;

        _overlapBuffer = new Collider[INIT_OVERLAP_BUFFER_SIZE];
    }

    public Vector3 DepenetrateFromColliders(Vector3 playerPosition)
    {
        int depenMaxIter = _playerMovementTuning.DEPEN_MAX_ITER;
        float depenEPS = _playerMovementTuning.DEPEN_EPS;
        var QTI = _playerMovementTuning.QTI;

        float baseMaxCorrection = Mathf.Max(_playerMovementTuning.SKIN_WIDTH * 4f, _playerMovementTuning.PROBE_UP_EXTENT);

        for (int i = 0; i < depenMaxIter; i++)
        {
            _capsuleGeometryService.GetCapsulePoints(playerPosition, out Vector3 p1, out Vector3 p2);

            int count = OverlapCapsule(p1, p2, _playerCollider.radius,
                                                          _worldMask, QTI);

            if (count <= 0)
            {
                return playerPosition;
            }

            CapsuleCollider playerCol = _playerCollider;
            Quaternion playerRotation = _playerCollider.transform.rotation;

            for (int j = 0; j < _overlapBuffer.Length; j++)
            {
                Collider col = _overlapBuffer[j];
                if (!col || col == playerCol)
                    continue;

                Vector3 colPos = col.transform.position;
                Quaternion colRot = col.transform.rotation;

                if (Physics.ComputePenetration(playerCol, playerPosition, playerRotation,
                                               col, colPos, colRot,
                                               out Vector3 pushDir, out float pushDist))
                {
                    bool isDynamic = col.attachedRigidbody && !col.attachedRigidbody.isKinematic;
                    float maxCorrection = isDynamic
                                          ? baseMaxCorrection
                                          : Mathf.Max(baseMaxCorrection, _playerMovementTuning.STEP_OFFSET);

                    float correctedDist = Mathf.Min(pushDist + depenEPS, maxCorrection);
                    playerPosition += pushDir * correctedDist;
                }
            }
        }

        return playerPosition;
    }

    private int OverlapCapsule(
        Vector3 p1,
        Vector3 p2,
        float radius,
        int layerMask,
        QueryTriggerInteraction queryTriggerInteraction)
    {
        while (true)
        {
            int count = Physics.OverlapCapsuleNonAlloc(
                p1, p2, radius,
                _overlapBuffer,
                layerMask,
                queryTriggerInteraction);

            if (count < _overlapBuffer.Length)
                return count;

            int newSize = Mathf.Min(_overlapBuffer.Length * 2, MAX_OVERLAP_BUFFER_SIZE);
            if (newSize == _overlapBuffer.Length)
                return count;

            _overlapBuffer = new Collider[newSize];
        }
    }
}
