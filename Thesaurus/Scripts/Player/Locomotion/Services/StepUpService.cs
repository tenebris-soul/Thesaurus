using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Zenject.SpaceFighter;
using static IGroundingService;

public class StepUpService : IStepUpService
{
    private readonly CapsuleCollider _playerCollider;
    private readonly LayerMask _worldMask;

    private readonly ICapsuleGeometryService _capsuleGeometryService;
    private readonly ISlopeChecker _slopeChecker;
    private readonly IGroundingService _groundingService;

    private readonly PlayerMovementTuning _playerMovementTuning;

    private readonly PlayerReadContext _playerReadMotorContext;

    private readonly IGizmosSphereService _gizmosSphere;
    private readonly RaycastHit[] _stepTopHits = new RaycastHit[16];

    private Collider[] _raisedOverlaps = new Collider[8];
    private Collider[] _stepOverlaps = new Collider[8];


    public StepUpService(CapsuleCollider playerCollider,
                         LayerMask worldMask,
                         ICapsuleGeometryService capsuleGeometryService,
                         ISlopeChecker slopeChecker,
                         IGroundingService groundingService,
                         PlayerMovementTuning playerMovementTuning,
                         PlayerReadContext playerWriteContext,
                         IGizmosSphereService gizmosSphere)
    {
        _playerCollider = playerCollider;
        _worldMask = worldMask;
        _capsuleGeometryService = capsuleGeometryService;
        _slopeChecker = slopeChecker;
        _groundingService = groundingService;
        _playerMovementTuning = playerMovementTuning;
        _playerReadMotorContext = playerWriteContext;
        _gizmosSphere = gizmosSphere;
    }

    public bool TryStepUp(ref Vector3 playerPosition, Vector3 moveDir, float moveDist)
    {
        bool isGrounded = _playerReadMotorContext.IsGrounded;
        bool wasGroundedInPrevFrame = _playerReadMotorContext.WasGroundedInPrevFrame;
        bool isSteepGround = _playerReadMotorContext.IsSteepGround;

        if (!isGrounded && !wasGroundedInPrevFrame) return false;
        if (isSteepGround) return false;
        if (moveDist <= 1e-3f) return false;

        float skinWidth     = _playerMovementTuning.SKIN_WIDTH;
        float stepOffset    = _playerMovementTuning.STEP_OFFSET;
        float probeUpExtent = _playerMovementTuning.PROBE_UP_EXTENT;
        var   QTI           = _playerMovementTuning.QTI;

        Vector3 up = _playerCollider.transform.up;

        moveDir = Vector3.ProjectOnPlane(moveDir, up);
        if (moveDir.sqrMagnitude <= 1e-12f) return false;
        moveDir.Normalize();

        _capsuleGeometryService.GetCapsulePoints(playerPosition, out Vector3 top0, out Vector3 bot0);
        float radius = _playerCollider.radius;

    #if UNITY_EDITOR || DEVELOPMENT_BUILD
        DbgCapsuleApprox(top0, bot0, radius, new Color(0.8f, 0.8f, 0.8f, 1f));
    #endif

        float lowDistance = moveDist + skinWidth;

    #if UNITY_EDITOR || DEVELOPMENT_BUILD
        DbgSphereCast(bot0, radius, moveDir, lowDistance, Color.cyan);
        DbgPoint(bot0, Color.cyan, 0.03f);
        DbgRay(bot0, moveDir, lowDistance, new Color(0.2f, 1f, 1f, 1f));
    #endif

        if (!Physics.SphereCast(bot0, radius, moveDir, out RaycastHit lowHit, lowDistance, _worldMask, QTI))
            return false;

    #if UNITY_EDITOR || DEVELOPMENT_BUILD
        DbgHit(lowHit, Color.red, new Color(1f, 0.5f, 0.5f, 1f));
    #endif

        float forward = Mathf.Clamp(lowHit.distance - skinWidth, 0f, moveDist);

    #if UNITY_EDITOR || DEVELOPMENT_BUILD
        DbgRay(bot0, moveDir, forward, Color.green);
        DbgPoint(bot0 + moveDir * forward, Color.green, 0.035f);
    #endif

        Vector3 raisedPosInPlace = playerPosition + up * stepOffset;
        _capsuleGeometryService.GetCapsulePoints(raisedPosInPlace, out Vector3 raisedTop0, out Vector3 raisedBot0);

    #if UNITY_EDITOR || DEVELOPMENT_BUILD
        DbgPoint(raisedPosInPlace, Color.green, 0.04f);
        DbgCapsuleApprox(raisedTop0, raisedBot0, radius, new Color(0.2f, 1f, 0.2f, 1f));
    #endif

        int raisedOverlapCount = Physics.OverlapCapsuleNonAlloc(
            raisedTop0, raisedBot0, radius, _raisedOverlaps, _worldMask, QTI);

    #if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (raisedOverlapCount > 0)
            DbgPoint((raisedTop0 + raisedBot0) * 0.5f, new Color(1f, 0.2f, 0.2f, 1f), 0.06f);
    #endif

        if (raisedOverlapCount > 0) return false;

    #if UNITY_EDITOR || DEVELOPMENT_BUILD
        DbgCapsuleSweep(raisedTop0, raisedBot0, radius, moveDir, forward + skinWidth, Color.yellow);
    #endif

        if (Physics.CapsuleCast(raisedTop0, raisedBot0, radius, moveDir, out RaycastHit highHit, forward + skinWidth, _worldMask, QTI))
        {
    #if UNITY_EDITOR || DEVELOPMENT_BUILD
            DbgHit(highHit, new Color(1f, 0.6f, 0.1f, 1f), new Color(1f, 0.8f, 0.3f, 1f));
    #endif
            return false;
        }

        float forwardProbe = Mathf.Max(radius + skinWidth, 0.05f);

        Vector3 baseXZ = playerPosition + moveDir * forward;
        Vector3 probeOrigin = baseXZ + moveDir * forwardProbe + up * (stepOffset + probeUpExtent);

        float probeDownDist = stepOffset + probeUpExtent + 0.25f;

        float downProbeRadius = Mathf.Max(0.02f, radius * 0.55f);

        Vector3 right = Vector3.Cross(up, moveDir);
        if (right.sqrMagnitude > 1e-8f) right.Normalize();

        float side = downProbeRadius * 0.9f;

        RaycastHit downHit;
        bool hasTop =
            TryGetStepTopHit(probeOrigin, -up, probeDownDist, downProbeRadius, up, QTI, out downHit) ||
            TryGetStepTopHit(probeOrigin + right * side, -up, probeDownDist, downProbeRadius, up, QTI, out downHit) ||
            TryGetStepTopHit(probeOrigin - right * side, -up, probeDownDist, downProbeRadius, up, QTI, out downHit);

        if (!hasTop)
            return false;

    #if UNITY_EDITOR || DEVELOPMENT_BUILD
        DbgPoint(baseXZ, new Color(0.3f, 0.6f, 1f, 1f), 0.04f);
        DbgPoint(probeOrigin, Color.magenta, 0.045f);
        DbgRay(probeOrigin, -up, probeDownDist, Color.magenta);
    #endif

    #if UNITY_EDITOR || DEVELOPMENT_BUILD
        DbgHit(downHit, Color.white, new Color(0.7f, 0.7f, 1f, 1f));
    #endif

        float currentBottomY = bot0.y - radius;
        float wantedBottomY = downHit.point.y;
        float stepRise = wantedBottomY - currentBottomY;

    #if UNITY_EDITOR || DEVELOPMENT_BUILD
        Vector3 curFloor = new Vector3(bot0.x, currentBottomY, bot0.z);
        Vector3 wantFloor = new Vector3(baseXZ.x, wantedBottomY, baseXZ.z);
        DbgPoint(curFloor, new Color(1f, 1f, 1f, 0.7f), 0.03f);
        DbgPoint(wantFloor, new Color(1f, 1f, 1f, 1f), 0.03f);
        DbgSegment(curFloor, wantFloor, new Color(1f, 1f, 1f, 1f), 0.015f);
    #endif

        if (stepRise <= 1e-4f) return false;
        if (stepRise > stepOffset + 1e-2f) return false;

        if (Vector3.Dot(downHit.normal, up) < 0.6f)
            return false;

        Vector3 botToPos = playerPosition - bot0;
        Vector3 desiredBot = new Vector3(baseXZ.x, downHit.point.y + radius + skinWidth, baseXZ.z);
        Vector3 desiredPos = desiredBot + botToPos;

    #if UNITY_EDITOR || DEVELOPMENT_BUILD
        DbgPoint(desiredPos, Color.blue, 0.05f);
    #endif

        _capsuleGeometryService.GetCapsulePoints(desiredPos, out Vector3 finalTop, out Vector3 finalBot);

    #if UNITY_EDITOR || DEVELOPMENT_BUILD
        DbgCapsuleApprox(finalTop, finalBot, radius, Color.white);
    #endif

        int finalOverlaps = Physics.OverlapCapsuleNonAlloc(
            finalTop, finalBot, radius, _stepOverlaps, _worldMask, QTI);

    #if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (finalOverlaps > 0)
            DbgPoint((finalTop + finalBot) * 0.5f, new Color(1f, 0.2f, 0.2f, 1f), 0.08f);
    #endif

        if (finalOverlaps > 0) return false;

        playerPosition = desiredPos;
        return true;
    }

    private bool TryGetStepTopHit(
                            Vector3 origin,
                            Vector3 down,
                            float maxDist,
                            float probeRadius,
                            Vector3 up,
                            QueryTriggerInteraction QTI,
                            out RaycastHit bestHit)
    {
        bestHit = default;

        int count = Physics.SphereCastNonAlloc(
            origin,
            probeRadius,
            down,
            _stepTopHits,
            maxDist,
            _worldMask,
            QTI);

        if (count <= 0) return false;

        float bestDist = float.PositiveInfinity;
        bool found = false;

        for (int i = 0; i < count; i++)
        {
            var h = _stepTopHits[i];
            if (h.collider == null) continue;

            // отсекаем подступёнки/стены
            if (Vector3.Dot(h.normal, up) < 0.6f) 
                continue;

            if (h.distance < bestDist)
            {
                bestDist = h.distance;
                bestHit = h;
                found = true;
            }
        }

        return found;
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private const float DBG_DOT_R = 0.02f;

    private void DbgWireSphere(Vector3 p, float r, Color c)
        => _gizmosSphere?.AddSphere(p, r, c, solid: false);

    private void DbgSolidSphere(Vector3 p, float r, Color c)
        => _gizmosSphere?.AddSphere(p, r, c, solid: true);

    private void DbgPoint(Vector3 p, Color c, float r = 0.04f)
        => DbgSolidSphere(p, r, c);

    private void DbgSegment(Vector3 a, Vector3 b, Color c, float dotR = DBG_DOT_R, int maxDots = 28)
    {
        if (_gizmosSphere == null) return;

        float len = Vector3.Distance(a, b);
        if (len <= 1e-5f)
        {
            DbgPoint(a, c, dotR);
            return;
        }

        // Пунктир по длине, но с лимитом количества точек
        int dots = Mathf.Clamp(Mathf.CeilToInt(len / (dotR * 2.2f)), 2, maxDots);
        for (int i = 0; i < dots; i++)
        {
            float t = (float)i / (dots - 1);
            Vector3 p = Vector3.Lerp(a, b, t);
            DbgSolidSphere(p, dotR, c);
        }
    }

    private void DbgRay(Vector3 origin, Vector3 dir, float len, Color c, float dotR = DBG_DOT_R)
    {
        if (len <= 0f) return;
        DbgSegment(origin, origin + dir.normalized * len, c, dotR);
    }

    private void DbgCapsuleApprox(Vector3 top, Vector3 bot, float radius, Color c)
    {
        // капсула = 2 сферы + осевая линия (пунктир)
        DbgWireSphere(top, radius, c);
        DbgWireSphere(bot, radius, c);
        DbgSegment(top, bot, c, Mathf.Min(DBG_DOT_R, radius * 0.18f));
    }

    private void DbgSphereCast(Vector3 origin, float radius, Vector3 dir, float dist, Color c)
    {
        Vector3 end = origin + dir.normalized * Mathf.Max(0f, dist);
        DbgWireSphere(origin, radius, c);
        DbgWireSphere(end, radius, c);
        DbgSegment(origin, end, c, Mathf.Min(DBG_DOT_R, radius * 0.18f));
    }

    private void DbgCapsuleSweep(Vector3 top, Vector3 bot, float radius, Vector3 dir, float dist, Color c)
    {
        Vector3 d = dir.normalized * Mathf.Max(0f, dist);
        DbgCapsuleApprox(top, bot, radius, c);
        DbgCapsuleApprox(top + d, bot + d, radius, c);
        DbgSegment((top + bot) * 0.5f, (top + bot) * 0.5f + d, c, Mathf.Min(DBG_DOT_R, radius * 0.18f));
    }

    private void DbgHit(RaycastHit hit, Color pointColor, Color normalColor)
    {
        DbgPoint(hit.point, pointColor, 0.045f);
        DbgRay(hit.point, hit.normal, 0.25f, normalColor, 0.015f);
    }
#endif

}
