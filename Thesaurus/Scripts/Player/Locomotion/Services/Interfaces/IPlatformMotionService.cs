using UnityEngine;

public interface IPlatformMotionService
{
    Vector3 ComputeGroundPointVelocity(Vector3 pointWorldPos, float dt);
    Vector3 ApplyGroundVelocity(Vector3 playerPosition);

    void EndTick(Rigidbody groundRb);
}
