using UnityEngine;

public interface IPlayerRotation
{
    void AddInputYawAndPitch(float yaw, float pitch);
    void AddExternalYaw(float yaw);
    void RememberCameraTransform();
    void ApplyInitCameraTransform();
    void ChangeBlockRotation(bool block);
}
