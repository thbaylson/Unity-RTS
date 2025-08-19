using UnityEngine;

namespace RTS.Commands
{
    // We made this into an abstract class so that we can see it in the inspector.
    public abstract class ActionBase : ScriptableObject, ICommand
    {
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: Range(0, 8)] [field: SerializeField] public int Slot { get; private set; }
        [field: SerializeField] public bool RequiresClickToActivate { get; private set; } = true;

        public abstract bool CanHandle(CommandContext ctx);
        public abstract void Handle(CommandContext ctx);
    }
}