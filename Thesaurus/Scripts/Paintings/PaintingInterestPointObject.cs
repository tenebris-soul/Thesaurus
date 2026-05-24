using UnityEngine;

public class PaintingInterestPointObject : MonoBehaviour
{
    [SerializeField] private PaintingInterestPointData _interestData;
    public PaintingInterestPointData InterestData => _interestData;

    private Collider _collider;
    public Collider Collider => _collider;

    private void OnValidate() 
    {
        CacheCollider();
    }
    private void Awake()
    {
        CacheCollider();
    }

    private void CacheCollider()
    {
        if(_collider == null)
            _collider = GetComponent<Collider>();   
    }
}
