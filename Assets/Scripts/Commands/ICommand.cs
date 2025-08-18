namespace RTS.Commands
{
    public interface ICommand
    {
        bool CanHandle(CommandContext ctx);
        void Handle(CommandContext ctx);
    }
}