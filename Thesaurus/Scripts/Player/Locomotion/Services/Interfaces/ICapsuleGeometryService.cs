using UnityEngine;

public interface ICapsuleGeometryService
{
    void GetCapsulePoints(Vector3 pos, out Vector3 p1, out Vector3 p2);
}
