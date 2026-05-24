using UnityEngine;

public interface IGizmosSphereService : IGizmosRenderable
{
    void AddSphere(Vector3 position, float radius, Color color, bool solid = false);
    void Clear();
}