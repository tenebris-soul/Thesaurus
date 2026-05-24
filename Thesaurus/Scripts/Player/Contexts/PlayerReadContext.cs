using UnityEngine;

public interface PlayerReadContext
{
    // jump options
    bool RequestJumping { get; }

    // grounding states
    Vector3 GroundNormal { get; }
    Collider GroundCollider { get; }
    bool IsGrounded { get; }
    bool WasGroundedInPrevFrame { get; }
    bool IsSteepGround { get; }
    bool IsLanded { get; }
    bool LeftGround { get; }
    bool IsHittingCeiling { get; }
}
