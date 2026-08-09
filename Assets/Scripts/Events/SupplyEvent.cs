using RTS.EventBus;
using RTS.Environment;

namespace RTS.Events
{
    public struct SupplyEvent : IEvent
    {
        public int Amount { get; private set; }
        public SupplySO Supply { get; private set; }

        public SupplyEvent(int amount, SupplySO supply)
        {
            Amount = amount;
            Supply = supply;
        }
    }
}