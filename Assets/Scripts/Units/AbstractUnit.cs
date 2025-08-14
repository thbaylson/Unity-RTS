using RTS.EventBus;
using RTS.Events;
using UnityEngine;
using UnityEngine.AI;

namespace RTS.Units
{
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class AbstractUnit : AbstractCommandable, IMoveable
    {
        public float AgentRadius => agent.radius;
        private NavMeshAgent agent;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        private void Start()
        {
            Bus<UnitSpawnedEvent>.Raise(new UnitSpawnedEvent(this));
        }

        public void MoveTo(Vector3 position)
        {
            agent.SetDestination(position);
        }
    }
}