using RTS.Environment;
using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

namespace RTS.Behavior
{

#if UNITY_EDITOR
    [CreateAssetMenu(menuName = "Behavior/Event Channels/GatherSuppliesEvent")]
#endif
    [Serializable, GeneratePropertyBag]
    [EventChannelDescription(name: "GatherSuppliesEventChannel", message: "[Self] gathers [Amount] [Supplies] .", category: "Events", id: "fa0244398c0e1e833fc9a9847f221e0c")]
    public partial class GatherSuppliesEventChannel : EventChannelBase
    {
        public delegate void GatherSuppliesEventEventHandler(GameObject Self, int Amount, SupplySO Supplies);
        public event GatherSuppliesEventEventHandler Event;

        public void SendEventMessage(GameObject Self, int Amount, SupplySO Supplies)
        {
            Event?.Invoke(Self, Amount, Supplies);
        }

        public override void SendEventMessage(BlackboardVariable[] messageData)
        {
            BlackboardVariable<GameObject> SelfBlackboardVariable = messageData[0] as BlackboardVariable<GameObject>;
            var Self = SelfBlackboardVariable != null ? SelfBlackboardVariable.Value : default;

            BlackboardVariable<int> AmountBlackboardVariable = messageData[1] as BlackboardVariable<int>;
            var Amount = AmountBlackboardVariable != null ? AmountBlackboardVariable.Value : default;

            BlackboardVariable<SupplySO> SuppliesBlackboardVariable = messageData[2] as BlackboardVariable<SupplySO>;
            var Supplies = SuppliesBlackboardVariable != null ? SuppliesBlackboardVariable.Value : default;

            Event?.Invoke(Self, Amount, Supplies);
        }

        public override Delegate CreateEventHandler(BlackboardVariable[] vars, System.Action callback)
        {
            GatherSuppliesEventEventHandler del = (Self, Amount, Supplies) =>
            {
                BlackboardVariable<GameObject> var0 = vars[0] as BlackboardVariable<GameObject>;
                if (var0 != null)
                    var0.Value = Self;

                BlackboardVariable<int> var1 = vars[1] as BlackboardVariable<int>;
                if (var1 != null)
                    var1.Value = Amount;

                BlackboardVariable<SupplySO> var2 = vars[2] as BlackboardVariable<SupplySO>;
                if (var2 != null)
                    var2.Value = Supplies;

                callback();
            };
            return del;
        }

        public override void RegisterListener(Delegate del)
        {
            Event += del as GatherSuppliesEventEventHandler;
        }

        public override void UnregisterListener(Delegate del)
        {
            Event -= del as GatherSuppliesEventEventHandler;
        }
    }
}