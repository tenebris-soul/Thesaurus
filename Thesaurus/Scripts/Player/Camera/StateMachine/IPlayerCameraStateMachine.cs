public interface IPlayerCameraStateMachine
{
    void SwitchState<T>() where T : IPlayerCameraState;
}
