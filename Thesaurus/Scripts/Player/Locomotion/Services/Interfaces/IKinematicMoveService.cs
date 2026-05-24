using UnityEngine;

public interface IKinematicMoveService
{
    Vector3 MoveWithCollisions(Vector3 playerPosition,
                               Vector3 delta
                               //bool isGrounded,
                               //bool wasGroundedInPrevFrame,
                               //bool isSteepGround
                               );
}
