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
        private NavMeshAgent navAgent;
        private BehaviorGraphAgent graphAgent;

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

            // This prevents hand-placed objects from navigating to the origin on game start.
            MoveTo(transform.position);
        }

        protected override void Start()
        {
            base.Start();
            Bus<UnitSpawnedEvent>.Raise(new UnitSpawnedEvent(this));

            // This prevents runtime objects from navigating to the origin on spawn.
            MoveTo(transform.position);
        }

        public void MoveTo(Vector3 position)
        {
            graphAgent.SetVariableValue("TargetLocation", position);
        }
    }
}