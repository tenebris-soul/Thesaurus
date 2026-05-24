public interface IInputContext
{
    string ActionMap { get; }
    void Enable();
    void Disable();  
    void ReadAndDispatch();
} 

public interface IInputContext<T> : IInputContext
{
    void Subscribe(T receiver);
    void Unsubscribe(T receiver);
}
