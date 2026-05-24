using UnityEngine;

public interface PlayerWriteContext
{
    void SetRequestJumping(bool requested);
    void SetGroundCollider(Collider groundCollider);
    void SetGroundedState(bool isGrounded);
    void SetGroundedInPrevFrame(bool wasGrounded);
    void SetSteepGround(bool isSteepGround);
    void SetGroundNormal(Vector3 normal);
    void SetCeilingHitting(bool isHittingCeiling);
}
