using UnityEngine;

public interface IGroundingService
{
    void ProbeGround(Vector3 playerPosition,
                     out Rigidbody groundRb);
    Vector3 SoftSnapToGround(Vector3 playerPosition,
                                    Vector3 velocity);
}
