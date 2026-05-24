using UnityEngine;

public class SlopeChecker : ISlopeChecker
{
    private readonly PlayerMovementTuning _playerMovementTuning;

    public SlopeChecker(PlayerMovementTuning playerMovementTuning)
    {
        _playerMovementTuning = playerMovementTuning;
    }

    public bool CheckSlopeAngle(Vector3 surfaceNormal)
    {
        float maxAngle = _playerMovementTuning.MAX_SLOPE_ANGLE - _playerMovementTuning.SLOPE_ANGLE_EPS;
        maxAngle = Mathf.Clamp(maxAngle, 0f, 89.9f);
        return Vector3.Angle(surfaceNormal, Vector3.up) < maxAngle;
    }
}
