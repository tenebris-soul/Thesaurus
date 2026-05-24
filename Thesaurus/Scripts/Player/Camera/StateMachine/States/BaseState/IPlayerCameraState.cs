public interface IPlayerCameraState
{
    void Enter();
    void Exit();
    void Tick();
    void LateTick();
    void FixedTick();
}
