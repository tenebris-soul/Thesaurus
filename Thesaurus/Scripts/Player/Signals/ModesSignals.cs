public struct SearchModeSignal
{
    public bool IsActive;
}

public struct InspectModeSignal
{
    public bool IsActive;
    public ExhibitObject Exhibit;
}

public struct ArtefactModeSignal
{
    public bool IsActive;
    public ArtifactObject Artifact;
}

public struct PaintingModeSignal
{
    public bool IsActive;
    public PaintingObject Painting;
}