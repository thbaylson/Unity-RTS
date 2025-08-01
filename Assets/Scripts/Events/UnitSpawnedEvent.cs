using RTS.EventBus;
using RTS.Units;

namespace RTS.Events
{
    public struct UnitSpawnedEvent : IEvent
    {
        public AbstractUnit Unit { get; private set; }

        public UnitSpawnedEvent(AbstractUnit unit)
        {
            Unit = unit;
        }
    }
}