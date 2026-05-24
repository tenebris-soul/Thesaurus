using UnityEngine;

[CreateAssetMenu(menuName = "Exhibits/ExhibitData")]
public class ExhibitData : ScriptableObject
{
    public string Name;

    [TextArea(3,20)]
    public string Description;
}
