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
    [NodeDescription(name: "Stop Agent", story: "[Agent] stops moving.", category: "Action/Navigation", id: "6c77f6412d31c4148c4900cf6ddaba7a")]
    public partial class StopAgentAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;

        protected override Status OnStart()
        {
            if (Agent.Value.TryGetComponent(out NavMeshAgent agent))
            {
                if (agent.TryGetComponent(out Animator animator))
                {
                    animator.SetFloat(AnimationConstants.SPEED, 0f);
                }

                agent.ResetPath();
                return Status.Success;
            }

            return Status.Failure;
        }
    }
}