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
        }

        protected override void Start()
        {
            base.Start();
            Bus<UnitSpawnedEvent>.Raise(new UnitSpawnedEvent(this));
        }

        public void MoveTo(Vector3 position)
        {
            graphAgent.SetVariableValue("TargetLocation", position);
        }
    }
}