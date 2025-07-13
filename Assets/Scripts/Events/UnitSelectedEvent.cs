using RTS.EventBus;
using RTS.Units;

namespace RTS.Events
{
    public struct UnitSelectedEvent : IEvent
    {
        public ISelectable Unit { get; private set; }

        public UnitSelectedEvent(ISelectable unit)
        {
            Unit = unit;
        }
    }
}