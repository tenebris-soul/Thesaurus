using UnityEngine;

public class CapsuleGeometryService : ICapsuleGeometryService
{
    private readonly Rigidbody _playerRigidbody;
    private readonly CapsuleCollider _playerCollider;

    public CapsuleGeometryService(Rigidbody playerRigidbody, CapsuleCollider playerCollider)
    {
        _playerRigidbody = playerRigidbody;
        _playerCollider = playerCollider;
    }

    public void GetCapsulePoints(Vector3 pos, out Vector3 p1, out Vector3 p2)
    {
        Vector3 center = _playerRigidbody.rotation * _playerCollider.center;

        float radius = _playerCollider.radius;
        float halfHeight = _playerCollider.height * 0.5f;
        float cyl = halfHeight - radius;

        Vector3 up = Vector3.up;

        p1 = pos + center + up * cyl;
        p2 = pos + center - up * cyl;
    }
}
