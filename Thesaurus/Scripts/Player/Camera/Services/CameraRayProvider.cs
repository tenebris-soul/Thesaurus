using UnityEngine;
using UnityEngine.InputSystem;

public sealed class CameraRayProvider : ICameraRayProvider
{
    private Transform _cameraTransform;
    
    public CameraRayProvider(Transform cameraTransform)
    {
        _cameraTransform = cameraTransform;
    }

    public Ray GetForwardRay()
    {
        return new Ray(_cameraTransform.position, _cameraTransform.forward);
    }

    public Ray GetMouseRay()
    {
        var mouse = Mouse.current;
        return Camera.main.ScreenPointToRay(mouse.position.ReadValue());
    }
}
