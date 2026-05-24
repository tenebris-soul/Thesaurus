using UnityEngine;

public interface IPlayerMotor
{
    Vector3 Velocity { get; }
    void SetVelocity(Vector3 velocity);
    (Vector3, Vector3) GetPlayerDirs();
}
