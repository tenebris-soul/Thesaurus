using UnityEngine;

public class CubeRotation : MonoBehaviour
{
    [SerializeField] private float _angleSpeed = 30f;
    [SerializeField] private float _speed = 2f;

    private Rigidbody _rb;

    private Vector3 _startPosition;
    void Start()
    {
        _rb = GetComponent<Rigidbody>();

        _startPosition = _rb.position;
    }

    void FixedUpdate()
    {
        Quaternion rbRot = transform.localRotation;
        Quaternion deltaRot = Quaternion.AngleAxis(_angleSpeed * Time.fixedDeltaTime, Vector3.up);

        _rb.MoveRotation (deltaRot * rbRot);

        float y = Mathf.PingPong(Time.time * _speed, 5f);
        _rb.MovePosition(_startPosition + Vector3.up * y);
    }
}
