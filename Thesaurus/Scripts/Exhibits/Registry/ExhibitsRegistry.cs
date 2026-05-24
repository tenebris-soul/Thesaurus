using System.Collections.Generic;
using UnityEngine;

public static class ExhibitsRegistry
{
    static readonly Dictionary<Collider, ExhibitObject> _map = new Dictionary<Collider, ExhibitObject>();

    public static void RegisterExhibit(Collider collider, ExhibitObject exhibit)
    {
        if(collider) _map[collider] = exhibit;
    }

    public static void UnregisterExhibit(Collider collider)
    {
        if(collider) _map.Remove(collider);
    }

    public static bool TryGetExhibit(Collider collider, out ExhibitObject exhibit)
    {
        if(collider)
        {
            return _map.TryGetValue(collider, out exhibit);
        }

        exhibit = null;
        return false;
    }
}
