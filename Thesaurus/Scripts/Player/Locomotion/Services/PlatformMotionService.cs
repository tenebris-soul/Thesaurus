using UnityEngine;

public class PlatformMotionService : IPlatformMotionService
{
    private readonly IPlayerRotation _playerRotation;

    private readonly PlayerReadContext _playerReadMotorContext;

    private Rigidbody _groundRb;
    private Vector3 _groundPrevPos;
    private Quaternion _groundPrevRot;

    public PlatformMotionService(IPlayerRotation playerRotation, PlayerReadContext playerReadMotorContext)
    {
        _playerRotation = playerRotation;
        _playerReadMotorContext = playerReadMotorContext;
    }

    public Vector3 ApplyGroundVelocity(Vector3 playerPosition)
    {
        if (_groundRb)
        {
            Quaternion deltaRot = _groundRb.rotation * Quaternion.Inverse(_groundPrevRot);
            deltaRot.ToAngleAxis(out float angleDeg, out Vector3 axis);

            if (angleDeg > 180f) angleDeg -= 360f;
            float absAngle = Mathf.Abs(angleDeg);

            const float MinRotDeg = 0.1f;
            const float MaxRotDeg = 20f;

            if (axis.sqrMagnitude < 1e-8f || absAngle < MinRotDeg)
            {
                deltaRot = Quaternion.identity;
            }
            else if (absAngle > MaxRotDeg)
            {
                deltaRot = Quaternion.AngleAxis(Mathf.Sign(angleDeg) * MaxRotDeg, axis.normalized);
            }

            playerPosition = _groundRb.position + deltaRot * (playerPosition - _groundPrevPos);

            Vector3 a = Vector3.ProjectOnPlane(_groundPrevRot * Vector3.forward, Vector3.up);
            Vector3 b = Vector3.ProjectOnPlane(_groundRb.rotation * Vector3.forward, Vector3.up);
            float deltaYaw = Vector3.SignedAngle(a, b, Vector3.up);

            const float MinYawDeg = 0.1f;
            const float MaxYawDeg = 20f;
            if (Mathf.Abs(deltaYaw) >= MinYawDeg)
            {
                deltaYaw = Mathf.Clamp(deltaYaw, -MaxYawDeg, MaxYawDeg);
                _playerRotation.AddExternalYaw(deltaYaw);
            }
        }

        return playerPosition;
    }

    public Vector3 ComputeGroundPointVelocity(Vector3 pointWorldPos, float dt)
    {
        Vector3 linear = (_groundRb.position - _groundPrevPos) / dt;

        Quaternion dq = _groundRb.rotation * Quaternion.Inverse(_groundPrevRot);
        dq.ToAngleAxis(out float angleDeg, out Vector3 axis);

        if (angleDeg > 180f) angleDeg -= 360f;

        Vector3 omega = Vector3.zero;
        if (axis.sqrMagnitude > 1e-8f && Mathf.Abs(angleDeg) > 1e-5f)
            omega = axis.normalized * (angleDeg * Mathf.Deg2Rad / dt);

        Vector3 r = pointWorldPos - _groundRb.position;
        Vector3 angular = Vector3.Cross(omega, r);

        return linear + angular;
    }

    public void EndTick(Rigidbody groundRb)
    {
        _groundRb = _playerReadMotorContext.IsGrounded ? groundRb : null; 

        if (_groundRb) 
        { 
            _groundPrevPos = _groundRb.position; 
            _groundPrevRot = _groundRb.rotation; 
        }
    }
}
