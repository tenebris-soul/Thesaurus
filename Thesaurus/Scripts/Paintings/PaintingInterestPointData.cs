using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Painting/PaintingInterestPointData")]
public class PaintingInterestPointData : ScriptableObject
{
    [TextArea(3,20)]
    public string Description;
}
