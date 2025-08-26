using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS.Units
{
    public class BaseBuilding : AbstractCommandable
    {
        private Queue<UnitSO> buildingQueue = new(MAX_QUEUE_SIZE);
        private const int MAX_QUEUE_SIZE = 5;

        public void BuildUnit(UnitSO unit)
        {
            if(buildingQueue.Count >= MAX_QUEUE_SIZE)
            {
                Debug.LogWarning("BaseBuilding.BuildUnit() called while queue is full.");
                return;
            }

            buildingQueue.Enqueue(unit);
            if(buildingQueue.Count == 1)
            {
                StartCoroutine(DoBuildUnits());
            }
        }

        private IEnumerator DoBuildUnits()
        {
            while(buildingQueue.Count > 0)
            {
                yield return new WaitForSeconds(buildingQueue.Peek().BuildTime);
                Instantiate(buildingQueue.Dequeue().Prefab, transform.position, Quaternion.identity);
            }
        }
    }
}