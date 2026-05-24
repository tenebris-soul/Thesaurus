public interface IGameplayInputReceiver
{
    void OnInput(in GameplayInputFrame frame);
}

public interface IInspectInputReceiver
{
    void OnInput(in InspectInputFrame frame);
}

public interface IArtefactInputReceiver
{
    void OnInput(in ArtifactInputFrame frame);
}

public interface IPaintingInputReceiver
{
    void OnInput(in PaintingInputFrame frame);
}