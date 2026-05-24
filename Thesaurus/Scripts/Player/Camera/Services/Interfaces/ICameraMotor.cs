using UnityEngine;

public interface ICameraMotor
{
    void AddYawPitch(Vector2 input);
    void BlockChangeTransform();
    void ReleaseChangeTransform();
}
