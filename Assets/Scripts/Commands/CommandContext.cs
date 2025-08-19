using RTS.Units;
using UnityEngine;

namespace RTS.Commands
{
    // These objects may be created and destroyed frequently. Making them structs will create less garbage.
    public struct CommandContext
    {
        public AbstractCommandable Commandable { get; private set; }
        public RaycastHit Hit { get; private set; }
        public int UnitIndex { get; private set; }

        public CommandContext(AbstractCommandable commandable, RaycastHit hit, int unitIndex= 0)
        {
            Commandable = commandable;
            Hit = hit;
            UnitIndex = unitIndex;
        }
    }
}