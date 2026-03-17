using RTS.Environment;
using RTS.Utilities;
using System;
using System.Linq;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

namespace RTS.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Move to GatherableSupply", story: "[Agent] moves to [Supply] or nearby not busy supply.", category: "Action/Navigation", id: "dd7d1af6322a257a515841713fb3b029")]
    public partial class MoveToGatherableSupplyAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<GatherableSupply> Supply;
        [SerializeReference] public BlackboardVariable<float> SearchRadius = new(7);

        private NavMeshAgent navAgent;
        private Animator animator;
        private LayerMask suppliesLayerMask;
        private SupplySO supplySO;

        protected override Status OnStart()
        {
            if (!HasValidInputs()) return Status.Failure;

            Agent.Value.TryGetComponent(out animator);

            suppliesLayerMask = LayerMask.GetMask("Supplies");
            navAgent.SetDestination(GetAgentDestination());
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (animator != null)
            {
                animator.SetFloat(AnimationConstants.SPEED, navAgent.velocity.magnitude);
            }

            if (navAgent.pathPending || navAgent.remainingDistance > navAgent.stoppingDistance)
            {
                return Status.Running;
            }

            if (Supply.Value != null && !Supply.Value.IsBusy && Supply.Value.Amount > 0)
            {
                return Status.Success;
            }

            Collider[] colliders = FindNearbyNotBusyColliders();
            if (colliders.Length > 0)
            {
                Array.Sort(colliders, new ClosestColliderComparer(navAgent.transform.position));
                Supply.Value = colliders[0].GetComponent<GatherableSupply>();
                navAgent.SetDestination(GetAgentDestination());
                return Status.Running;
            }

            return Status.Failure;
        }

        protected override void OnEnd()
        {
            if (animator != null)
            {
                animator.SetFloat(AnimationConstants.SPEED, 0f);
            }
        }

        private bool HasValidInputs()
        {
            if (!Agent.Value.TryGetComponent(out navAgent) || (Supply.Value == null && supplySO == null)) return false;

            if (Supply.Value != null)
            {
                supplySO = Supply.Value.Supply;
            }
            else
            {
                Collider[] colliders = FindNearbyNotBusyColliders();
                if (colliders.Length > 0)
                {
                    Array.Sort(colliders, new ClosestColliderComparer(navAgent.transform.position));
                    Supply.Value = colliders[0].GetComponent<GatherableSupply>();
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private Collider[] FindNearbyNotBusyColliders()
        {
            return Physics.OverlapSphere(
                navAgent.transform.position,
                SearchRadius.Value,
                suppliesLayerMask
            ).Where(collider =>
                collider.TryGetComponent(out GatherableSupply supply)
                && !supply.IsBusy
                && supply.Supply.Equals(supplySO)
            ).ToArray();
        }

        private Vector3 GetAgentDestination()
        {
            // If the target has a collider, then find the closest point on the collider to the navAgent. Otherwise, just use the target's position.
            return Supply.Value.TryGetComponent(out Collider targetCollider)
                ? targetCollider.ClosestPoint(navAgent.transform.position)
                : Supply.Value.transform.position;
        }
    }
}