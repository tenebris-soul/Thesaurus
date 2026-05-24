using System.Collections.Generic;
using UnityEngine;

public static class ArtifactRegistry
{
    static readonly Dictionary<Collider, ArtifactObject> _map = new();

    public static void RegisterArtifact(Collider collider, ArtifactObject artifact)
    {
        if(collider) _map[collider] = artifact;
    }

    public static void UnregisterArtifact(Collider collider)
    {
        if(collider) _map.Remove(collider);
    }

    public static bool TryGetArtifact(Collider collider, out ArtifactObject artifact)
    {
        if(collider)
        {
            return _map.TryGetValue(collider, out artifact);
        }

        artifact = null;
        return false;
    }
}