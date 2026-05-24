using UnityEngine;
using Zenject;

public class PlayerRotation : IPlayerRotation, IFixedTickable, ILateTickable
{
    private float _inputYaw;
    private float _inputPitch;

    private float _externalYaw;

    private Transform _cameraAttachment;
    private Rigidbody _playerRigidbody;

    private Vector3 _initLocalPos;
    private Quaternion _initLocalRot;

    private bool _rotationBlocked = false;

    public PlayerRotation(Transform cameraAttachment,
                          Rigidbody playerRigidbody)
    {
        _cameraAttachment = cameraAttachment;
        _playerRigidbody = playerRigidbody;
    }

    public void AddExternalYaw(float yaw) => _externalYaw = yaw;

    public void AddInputYawAndPitch(float yaw, float pitch)
    {
        _inputYaw += yaw;
        _inputPitch = Mathf.Clamp(_inputPitch + pitch, -90f, 60f);
    }

    public void FixedTick()
    {
        if(_rotationBlocked) return;

        Quaternion playerRot = _playerRigidbody.rotation;

        _playerRigidbody.MoveRotation(playerRot * Quaternion.Euler(0f, _inputYaw + _externalYaw, 0f));

        _inputYaw = 0f;
        _externalYaw = 0f;
    }

    public void LateTick()
    {
        if(_rotationBlocked) return;

        _cameraAttachment.localRotation = Quaternion.Euler(_inputPitch, 0f, 0f);
    }

    public void RememberCameraTransform()
    {
        _initLocalPos = _cameraAttachment.localPosition;
        _initLocalRot = _cameraAttachment.localRotation;
    }

    public void ApplyInitCameraTransform()
    {
        _cameraAttachment.localPosition = _initLocalPos;
        _cameraAttachment.localRotation = _initLocalRot;
    }

    public void ChangeBlockRotation(bool block) => _rotationBlocked = block;
}
