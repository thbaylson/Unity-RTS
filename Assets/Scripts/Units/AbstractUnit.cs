using RTS.EventBus;
using RTS.Events;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

namespace RTS.Units
{
    [RequireComponent(typeof(NavMeshAgent), typeof(BehaviorGraphAgent))]
    public abstract class AbstractUnit : AbstractCommandable, IMoveable
    {
        public float AgentRadius => navAgent.radius;
        protected BehaviorGraphAgent graphAgent;
        private NavMeshAgent navAgent;

        private void Awake()
        {
            navAgent = GetComponent<NavMeshAgent>();
            graphAgent = GetComponent<BehaviorGraphAgent>();
            if (graphAgent.Graph == null)
            {
                // We require the BehaviorGraphAgent component, but that doesn't guarantee that a Behavior Graph asset is assigned.
                Debug.LogError($"Behavior Graph property not found on object: {transform.name}.");
                return;
            }

            graphAgent.SetVariableValue("Command", UnitCommands.Stop);
        }

        protected override void Start()
        {
            base.Start();
            Bus<UnitSpawnedEvent>.Raise(new UnitSpawnedEvent(this));
        }

        public void MoveTo(Vector3 position)
        {
            graphAgent.SetVariableValue("TargetLocation", position);

            // Note: Changing the Command triggers an abort. We should update whatever data the next 
            // command needs before we switch to it, otherwise we might run into issues with the previous command's data still hanging around.
            graphAgent.SetVariableValue("Command", UnitCommands.Move);
        }

        public void Stop()
        {
            graphAgent.SetVariableValue("Command", UnitCommands.Stop);
        }
    }
}