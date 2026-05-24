public interface IInputRouter
{
    void Subscribe<T>(InputMode mode, T receiver);
    void Unsubscribe<T>(InputMode mode, T receiver);
    InputRouter.ModeLease PushMode(InputMode mode, object owner);
}