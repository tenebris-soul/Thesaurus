using UnityEngine;

public struct ArtifactInputFrame
{
    public readonly bool RotateHold;
    public readonly bool InteractPerformed;
    public readonly Vector2 RotateDelta;
    public readonly float Zoom;
    public readonly bool CancelPressed;

    public ArtifactInputFrame(bool rotateHold,
                              bool interactPerformed,
                              Vector2 rotateDelta,
                              float zoomPressed,
                              bool cancelPressed)
    {
        RotateHold = rotateHold;
        InteractPerformed = interactPerformed;
        RotateDelta = rotateDelta;
        Zoom = zoomPressed;
        CancelPressed = cancelPressed;
    }
}