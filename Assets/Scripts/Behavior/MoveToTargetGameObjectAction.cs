using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

namespace RTS.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Move to Target GameObject", story: "[Agent] moves to [TargetGameObject] .", category: "Action/Navigation", id: "46fb37157fdb5bc2dda1b80e7f0c3a3c")]
    public partial class MoveToTargetGameObjectAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<GameObject> TargetGameObject;

        private NavMeshAgent navAgent;

        protected override Status OnStart()
        {
            if (!Agent.Value.TryGetComponent(out navAgent)) return Status.Failure;

            Vector3 targetPos = TargetGameObject.Value.transform.position;
            if (Vector3.Distance(navAgent.transform.position, targetPos) <= navAgent.stoppingDistance) return Status.Success;

            navAgent.SetDestination(targetPos);
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
            {
                return Status.Success;
            }

            return Status.Running;
        }
    }
}