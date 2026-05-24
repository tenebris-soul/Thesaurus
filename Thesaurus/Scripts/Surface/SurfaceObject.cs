using UnityEngine;

public class SurfaceObject : MonoBehaviour
{
    [SerializeField] private string _surfaceName;
    private Collider[] _colliders;

    public string SurfaceName => _surfaceName;

    void OnValidate() => CacheColliders();
    void Awake() => CacheColliders();

    public void OnEnable()
    {
        for(int i = 0; i < _colliders.Length; i++)
        {
            SurfaceRegistry.RegisterSurface(_colliders[i], this);
        }
    }

    public void OnDisable()
    {
        for(int i = 0; i < _colliders.Length; i++)
        {
            SurfaceRegistry.UnregisterSurface(_colliders[i]);
        }
    }

    private void CacheColliders()
    {
        if (_colliders == null || _colliders.Length == 0)
            _colliders = GetComponentsInChildren<Collider>(true);
    }
}
