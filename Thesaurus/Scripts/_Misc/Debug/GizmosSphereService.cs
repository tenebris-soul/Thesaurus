using System.Collections.Generic;
using UnityEngine;

public sealed class GizmosSphereService : IGizmosSphereService
{
    private struct Sphere
    {
        public Vector3 Position;
        public float Radius;
        public Color Color;
        public bool Solid;
    }

    private readonly List<Sphere> _spheres = new List<Sphere>(128);

    public void AddSphere(Vector3 position, float radius, Color color, bool solid = false)
    {
        if (radius <= 0f) return;

        _spheres.Add(new Sphere
        {
            Position = position,
            Radius = radius,
            Color = color,
            Solid = solid
        });
    }

    public void Clear()
    {
        _spheres.Clear();
    }

    public void DrawGizmos()
    {
        // Важно: Gizmos рисуются только когда Unity зовёт OnDrawGizmos у MonoBehaviour (см. прокси).
        for (int i = 0; i < _spheres.Count; i++)
        {
            var s = _spheres[i];

            Gizmos.color = s.Color;
            if (s.Solid)
                Gizmos.DrawSphere(s.Position, s.Radius);
            else
                Gizmos.DrawWireSphere(s.Position, s.Radius);
        }
    }
}