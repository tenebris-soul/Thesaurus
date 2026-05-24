using System.Collections.Generic;
using UnityEngine;

public static class PaintingsRegistry
{
    static readonly Dictionary<Collider, PaintingObject> _map = new();

    public static void RegisterPainting(Collider collider, PaintingObject painting)
    {
        if(collider) _map[collider] = painting;
    }

    public static void UnregisterPainting(Collider collider)
    {
        if(collider) _map.Remove(collider);
    }

    public static bool TryGetPainting(Collider collider, out PaintingObject painting)
    {
        if(collider)
        {
            return _map.TryGetValue(collider, out painting);
        }

        painting = null;
        return false;
    }
}