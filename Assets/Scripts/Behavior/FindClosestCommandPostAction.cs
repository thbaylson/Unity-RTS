using System;
using System.Collections.Generic;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;
using RTS.Units;

namespace RTS.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Find Closest Command Post", story: "[Unit] finds nearest [CommandPost] .", category: "Action/Units", id: "1b0c619424fee363ea66997484bfa7b8")]
    public partial class FindClosestCommandPostAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Unit;
        [SerializeReference] public BlackboardVariable<GameObject> CommandPost;
        [SerializeReference] public BlackboardVariable<float> SearchRadius = new(10);
        [SerializeReference] public BlackboardVariable<UnitSO> CommandPostSO;

        protected override Status OnStart()
        {
            // This seems like a weird way to do this. TODO: Find a more sophisticated solution.
            Collider[] colliders = Physics.OverlapSphere(Unit.Value.transform.position, SearchRadius.Value, LayerMask.GetMask("Buildings"));
            
            List<BaseBuilding> nearbyCommandPosts = new();
            foreach(Collider collider in colliders)
            {
                // Checking the SO only works if there's exactly one type of Command Post building.
                if (collider.TryGetComponent(out BaseBuilding building) && building.UnitSO.Equals(CommandPostSO.Value))
                {
                    nearbyCommandPosts.Add(building);
                }
            }

            if(nearbyCommandPosts.Count == 0)
            {
                return Status.Failure;
            }
            
            // This doesn't actually get the closest, just the first in the list.
            CommandPost.Value = nearbyCommandPosts[0].gameObject;

            return Status.Success;
        }
    }
}