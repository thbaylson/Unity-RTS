namespace RTS.UI
{
    public interface IUIElement<T>
    {
        void EnableFor(T context);
        void Disable();
    }

    public interface IUIElement<T1, T2>
    {
        void EnableFor(T1 context, T2 callback);
        void Disable();
    }
}