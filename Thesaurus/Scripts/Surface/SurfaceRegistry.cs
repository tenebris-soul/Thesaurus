using System.Collections.Generic;
using UnityEngine;

public static class SurfaceRegistry
{
    static readonly Dictionary<Collider, SurfaceObject> _map = new Dictionary<Collider, SurfaceObject>();

    public static void RegisterSurface(Collider collider, SurfaceObject surface)
    {
        if(collider) _map[collider] = surface;
    }

    public static void UnregisterSurface(Collider collider)
    {
        if(collider) _map.Remove(collider);
    }

    public static bool TryGetSurface(Collider collider, out SurfaceObject surface)
    {
        if(collider)
        {
            return _map.TryGetValue(collider, out surface);
        }

        surface = null;
        return false;
    }
}
