using UnityEngine;

public struct PaintingInputFrame
{
    public readonly bool MovePressed;
    public readonly Vector2 MoveDelta;
    public readonly float Zoom;
    public readonly bool CancelPressed;

    public PaintingInputFrame(bool movePressed, Vector2 moveDelta, float zoom, bool cancelPressed)
    {
        MovePressed = movePressed;
        Zoom = zoom;
        MoveDelta = moveDelta;
        CancelPressed = cancelPressed;
    }
}