using UnityEngine;

public struct GameplayInputFrame
{
    public readonly Vector2 Move;
    public readonly Vector2 LookDelta;
    public readonly bool IsJumping;
    public readonly bool InteractPressed;
    public readonly bool PausePressed;

    public GameplayInputFrame(Vector2 move,
                                Vector2 lookDelta,
                                bool isJumping,
                                bool interactPressed,
                                bool pausePressed)
    {
        Move = move;
        LookDelta = lookDelta;
        IsJumping = isJumping;
        InteractPressed = interactPressed;
        PausePressed = pausePressed;
    }
}