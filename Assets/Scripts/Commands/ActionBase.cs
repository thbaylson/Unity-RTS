using UnityEngine;

namespace RTS.Commands
{
    public abstract class ActionBase : ScriptableObject, ICommand
    {
        public abstract bool CanHandle(CommandContext ctx);
        public abstract void Handle(CommandContext ctx);
    }
}