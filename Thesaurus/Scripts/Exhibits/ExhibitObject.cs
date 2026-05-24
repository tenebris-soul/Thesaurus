using UnityEngine;

public class ExhibitObject : MonoBehaviour
{
    [SerializeField] private ExhibitData _exhibitData;
    [SerializeField] private Transform _cameraTarget;
    [SerializeField] private Transform _cameraAnchor;

    private Collider[] _colliders;

    public ExhibitData ExhibitData => _exhibitData;
    public Transform CameraTarget => _cameraTarget;
    public Transform CameraAnchor => _cameraAnchor;

    private void OnValidate() => CacheColliders();
    private void Awake() => CacheColliders();

    public void OnEnable()
    {
        for(int i = 0; i < _colliders.Length; i++)
        {
            ExhibitsRegistry.RegisterExhibit(_colliders[i], this);
        }
    }

    public void OnDisable()
    {
        for(int i = 0; i < _colliders.Length; i++)
        {
            ExhibitsRegistry.UnregisterExhibit(_colliders[i]);
        }
    }

    private void CacheColliders()
    {
        if (_colliders == null || _colliders.Length == 0)
            _colliders = GetComponentsInChildren<Collider>(true);
    }
}
