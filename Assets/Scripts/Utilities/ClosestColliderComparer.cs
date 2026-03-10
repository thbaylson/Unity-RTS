using System.Collections.Generic;
using UnityEngine;

namespace RTS.Utilities
{
    public struct ClosestColliderComparer : IComparer<Collider>
    {
        private Vector3 targetPos;

        public ClosestColliderComparer(Vector3 pos)
        {
            targetPos = pos;
        }

        public int Compare(Collider x, Collider y)
        {
            return (x.transform.position - targetPos).sqrMagnitude
                .CompareTo((y.transform.position - targetPos).sqrMagnitude);
        }
    }
}