using System.ComponentModel;
using UnityEngine;
using Zenject;

public class CameraMotor : ILateTickable, ICameraMotor
{
    private Transform _playerCameraTransform;
    private Transform _cameraAttach;
    private IPlayerRotation _playerRotation;

    private Vector3 _cameraVelocity;
    private float _smoothTime = 0.06f;
    private float _maxSpeed = 500f;
    private float _rotSharpness = 25f;

    private Vector3 _initPos;
    private Quaternion _initRot;

    public CameraMotor(Transform playerCameraTransform,
                       Transform cameraAttach,
                       IPlayerRotation playerRotation)
    {
        _playerCameraTransform = playerCameraTransform;
        _cameraAttach = cameraAttach;
        _playerRotation = playerRotation;
    }

    public void AddYawPitch(Vector2 input)
    {
        float yawDelta = input.x * Time.deltaTime * 30f;
        float pitchDelta = -input.y * Time.deltaTime * 30f;

        _playerRotation.AddInputYawAndPitch(yawDelta, pitchDelta);
    }

    public void LateTick()
    {
        MoveCameraToAttachment();
    }

    public void BlockChangeTransform()
    {
        _playerRotation.RememberCameraTransform();
        _playerRotation.ChangeBlockRotation(true);
    }

    public void ReleaseChangeTransform()
    {
        _playerRotation.ApplyInitCameraTransform();
        _playerRotation.ChangeBlockRotation(false);
    }

    private void MoveCameraToAttachment()
    {
        _playerCameraTransform.position = Vector3.SmoothDamp(_playerCameraTransform.position,
                                                              _cameraAttach.position,
                                                              ref _cameraVelocity,
                                                              _smoothTime,
                                                              _maxSpeed,
                                                              Time.deltaTime
                                                              );

        float t = 1f - Mathf.Exp(-_rotSharpness * Time.deltaTime);
        _playerCameraTransform.rotation = Quaternion.Slerp(_playerCameraTransform.rotation,
                                                           _cameraAttach.rotation,
                                                           t);
    }
}
