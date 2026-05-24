using UnityEngine;

public class ArtifactObject : MonoBehaviour
{
    [SerializeField] private ArtifactData artifactData;
    [SerializeField] private Collider[] _interestPoints;
    private bool[] _interestPointsFound;

    private Collider[] _colliders;

    public ArtifactData ArtifactData => artifactData;
    public Collider[] Colliders => _colliders;
    public Collider[] InterestPoints => _interestPoints;
    public bool[] InterestPointsFound => _interestPointsFound;

    private void OnValidate() => CacheColliders();
    private void Awake() 
    {
        CacheColliders();
        _interestPointsFound = new bool[_interestPoints.Length];
    } 

    public void FindInterestPoint(int index)
    {
        if(index < 0 || index >= _interestPoints.Length) return;
        if(_interestPointsFound[index]) return;

        _interestPointsFound[index] = true;
    }

    private void OnEnable()
    {
        for(int i = 0; i < _colliders.Length; i++)
        {
            ArtifactRegistry.RegisterArtifact(_colliders[i], this);
        }
    }

    private void OnDisable()
    {
        for(int i = 0; i < _colliders.Length; i++)
        {
            ArtifactRegistry.UnregisterArtifact(_colliders[i]);
        }
    }

    private void CacheColliders()
    {
        if (_colliders == null || _colliders.Length == 0)
            _colliders = GetComponentsInChildren<Collider>(true);
    }
}
