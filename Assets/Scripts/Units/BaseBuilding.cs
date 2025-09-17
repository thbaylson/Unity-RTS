using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS.Units
{
    public class BaseBuilding : AbstractCommandable
    {
        public UnitSO[] Queue => buildingQueue.ToArray();
        public int QueueSize => buildingQueue.Count;

        [field: SerializeField] public float CurrentQueueStartTime { get; private set; }
        [field: SerializeField] public UnitSO BuildingUnit { get; private set; }

        public delegate void QueueUpdatedEvent(UnitSO[] unitsInQueue);
        public event QueueUpdatedEvent OnQueueUpdated;

        private List<UnitSO> buildingQueue = new(MAX_QUEUE_SIZE);
        private const int MAX_QUEUE_SIZE = 5;

        public void BuildUnit(UnitSO unit)
        {
            if (buildingQueue.Count >= MAX_QUEUE_SIZE)
            {
                Debug.LogWarning("BaseBuilding.BuildUnit() called while queue is full.");
                return;
            }

            buildingQueue.Add(unit);
            if (buildingQueue.Count == 1)
            {
                StartCoroutine(DoBuildUnits());
            }
            else
            {
                OnQueueUpdated?.Invoke(buildingQueue.ToArray());
            }
        }

        public void CancelBuildingUnit(int index)
        {
            if (index < 0 || index >= buildingQueue.Count)
            {
                Debug.LogError("Cannot cancel building unit. Index out of range.");
                return;
            }

            buildingQueue.RemoveAt(index);
            if (index == 0)
            {
                StopAllCoroutines();
                if (buildingQueue.Count > 0)
                {
                    StartCoroutine(DoBuildUnits());
                }
                else
                {
                    OnQueueUpdated?.Invoke(buildingQueue.ToArray());
                }
            }
            else
            {
                OnQueueUpdated?.Invoke(buildingQueue.ToArray());
            }
        }


        private IEnumerator DoBuildUnits()
        {
            while (buildingQueue.Count > 0)
            {
                CurrentQueueStartTime = Time.time;
                BuildingUnit = buildingQueue[0];
                OnQueueUpdated?.Invoke(buildingQueue.ToArray());
                yield return new WaitForSeconds(BuildingUnit.BuildTime);

                Instantiate(BuildingUnit.Prefab, transform.position, Quaternion.identity);
                buildingQueue.RemoveAt(0);
            }

            OnQueueUpdated?.Invoke(buildingQueue.ToArray());
        }
    }
}