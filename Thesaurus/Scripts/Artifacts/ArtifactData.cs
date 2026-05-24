using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Artifacts/ArtifactData")]
public class ArtifactData : ScriptableObject
{
    public string Name;

    [TextArea(3,20)]
    public string Description;

    [TextArea(3,20)]
    public List<string> Facts;
}

