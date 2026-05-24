using UnityEngine;

public class PaintingObject : MonoBehaviour
{
    [SerializeField] private PaintingData _paintingData;
    [SerializeField] private PaintingAxis _axis;

    private PaintingInterestPointObject[] _interestPoints;
    private Collider[] _colliders;

    public PaintingData PaintingData => _paintingData;
    public PaintingAxis Axis => _axis;
    public Collider[] Colliders => _colliders;
    public PaintingInterestPointObject[] InterestPoints => _interestPoints;

    private void OnValidate() 
    {
        CacheColliders();
        CacheInterestPoints();
    }

    private void Awake()
    {
        CacheColliders();
        CacheInterestPoints();
    }

    public void OnEnable()
    {
        for(int i = 0; i < _colliders.Length; i++)
        {
            PaintingsRegistry.RegisterPainting(_colliders[i], this);
        }        
    }

    public void OnDisable()
    {
        for(int i = 0; i < _colliders.Length; i++)
        {
            PaintingsRegistry.UnregisterPainting(_colliders[i]);
        }
    }

    private void CacheColliders()
    {
        if(_colliders == null || _colliders.Length == 0)
            _colliders = GetComponentsInChildren<Collider>(true);   
    }
    private void CacheInterestPoints()
    {
        if(_interestPoints == null || _interestPoints.Length == 0)
            _interestPoints = GetComponentsInChildren<PaintingInterestPointObject>(true);
    }
}

public enum PaintingAxis
{
    X, NegativeX,
    Y, NegativeY,
    Z, NegativeZ
}
