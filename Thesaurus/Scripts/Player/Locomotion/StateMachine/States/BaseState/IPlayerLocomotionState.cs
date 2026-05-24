public interface IPlayerLocomotionState
{
    void Enter();
    void Exit();
    void Tick();
    void FixedTick();
    void SwitchSubstate<T>() where T : IPlayerLocomotionState;
}
