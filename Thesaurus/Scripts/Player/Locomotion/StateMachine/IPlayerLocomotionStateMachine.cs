public interface IPlayerLocomotionStateMachine 
{
    void SwitchState<T>() where T : IPlayerLocomotionState;
}
