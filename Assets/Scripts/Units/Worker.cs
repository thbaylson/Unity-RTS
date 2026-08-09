using RTS.Behavior;
using RTS.Environment;
using RTS.EventBus;
using RTS.Events;
using Unity.Behavior;
using UnityEngine;

namespace RTS.Units
{
    public class Worker : AbstractUnit
    {
        protected override void Start()
        {
            base.Start();
            if(graphAgent.GetVariable("GatherSuppliesEvent", out BlackboardVariable<GatherSuppliesEventChannel> eventChannelVariable))
            {
                eventChannelVariable.Value.Event += HandleGatherSupplies;
            }
        }

        public void Gather(GatherableSupply supply)
        {
            graphAgent.SetVariableValue("TargetGameObject", supply.gameObject);
            graphAgent.SetVariableValue("Supply", supply);

            graphAgent.SetVariableValue("Command", UnitCommands.Gather);
        }

        private void HandleGatherSupplies(GameObject self, int amount, SupplySO supply)
        {
            Bus<SupplyEvent>.Raise(new SupplyEvent(amount, supply));
        }
    }
}