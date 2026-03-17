using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;
using RTS.Utilities;

namespace RTS.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Move To Target Location", story: "[Agent] moves to [TargetLocation] .", category: "Action/Navigation", id: "64cbe0803223fff81dfe6a1404f4d22b")]
    public partial class MoveToTargetLocationAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<Vector3> TargetLocation;

        private NavMeshAgent navAgent;
        private Animator animator;

        protected override Status OnStart()
        {
            if (!Agent.Value.TryGetComponent(out navAgent))
            {
                return Status.Failure;
            }

            Agent.Value.TryGetComponent(out animator);

            if (Vector3.Distance(navAgent.transform.position, TargetLocation.Value) <= navAgent.stoppingDistance)
            {
                return Status.Success;
            }

            navAgent.SetDestination(TargetLocation.Value);

            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (animator != null)
            {
                animator.SetFloat(AnimationConstants.SPEED, navAgent.velocity.magnitude);
            }

            // remainingDistance will always be 0 while pathPending is true.
            if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
            {
                return Status.Success;
            }

            return Status.Running;
        }

        // OnEnd is called when we move away from this node, regardless of success or failure. It's safe to remove it if it's not needed.
        protected override void OnEnd()
        {
            if (animator != null)
            {
                animator.SetFloat(AnimationConstants.SPEED, 0f);
            }
        }
    }
}
