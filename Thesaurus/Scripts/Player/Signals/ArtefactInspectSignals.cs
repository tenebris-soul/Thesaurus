public struct ArtefactScrollPointedSignal
{
    public readonly bool IsPointing;

    public ArtefactScrollPointedSignal(bool isPointing)
    {
        IsPointing = isPointing;
    }
}

public struct ArtefactInterestPointFound
{
    public readonly int Index;

    public ArtefactInterestPointFound(int index)
    {
        Index = index;
    }
}