using RTS.Utilities;
using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

namespace RTS.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Move to Target GameObject", story: "[Agent] moves to [TargetGameObject] .", category: "Action/Navigation", id: "46fb37157fdb5bc2dda1b80e7f0c3a3c")]
    public partial class MoveToTargetGameObjectAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<GameObject> TargetGameObject;

        private NavMeshAgent navAgent;
        private Animator animator;

        protected override Status OnStart()
        {
            if (!Agent.Value.TryGetComponent(out navAgent) || TargetGameObject.Value == null) return Status.Failure;

            Agent.Value.TryGetComponent(out animator);

            // If the target has a collider, then find the closest point on the collider to the navAgent. Otherwise, just use the target's position.
            Vector3 targetPos = TargetGameObject.Value.TryGetComponent(out Collider targetCollider)
                ? targetCollider.ClosestPoint(navAgent.transform.position)
                : TargetGameObject.Value.transform.position;

            if (Vector3.Distance(navAgent.transform.position, targetPos) <= navAgent.stoppingDistance) return Status.Success;

            navAgent.SetDestination(targetPos);
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (animator != null)
            {
                animator.SetFloat(AnimationConstants.SPEED, navAgent.velocity.magnitude);
            }

            if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
            {
                return Status.Success;
            }

            return Status.Running;
        }

        protected override void OnEnd()
        {
            if (animator != null)
            {
                animator.SetFloat(AnimationConstants.SPEED, 0f);
            }
        }
    }
}