using UnityEngine;

public interface ICameraRayProvider
{
    Ray GetForwardRay();
    Ray GetMouseRay();
}