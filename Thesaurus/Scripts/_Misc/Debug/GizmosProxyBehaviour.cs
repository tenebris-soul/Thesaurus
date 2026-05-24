using UnityEngine;
using Zenject;

public sealed class GizmosProxyBehaviour : MonoBehaviour
{
    private IGizmosRenderable _renderable;

    [Inject]
    public void Construct(IGizmosRenderable renderable)
    {
        _renderable = renderable;
    }

    private void OnDrawGizmos()
    {
        _renderable?.DrawGizmos();
    }
}