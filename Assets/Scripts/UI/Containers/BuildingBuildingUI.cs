using RTS.UI.Components;
using RTS.Units;
using System;
using System.Collections;
using UnityEngine;

namespace RTS.UI.Containers
{
    public class BuildingBuildingUI : MonoBehaviour, IUIElement<BaseBuilding>
    {
        [SerializeField] private ProgressBar progressBar;

        private BaseBuilding building;
        private Coroutine buildCoroutine;

        public void EnableFor(BaseBuilding context)
        {
            gameObject.SetActive(true);
            building = context;
            building.OnQueueUpdated += HandleQueueUpdated;

            buildCoroutine = StartCoroutine(UpdateUnitProgress());
        }

        public void Disable()
        {
            if (building != null)
            {
                building.OnQueueUpdated -= HandleQueueUpdated;
            }

            gameObject.SetActive(false);
            building = null;
            buildCoroutine = null;
        }

        private void HandleQueueUpdated(UnitSO[] unitsInQueue)
        {
            if(unitsInQueue.Length == 1 && buildCoroutine == null)
            {
                buildCoroutine = StartCoroutine(UpdateUnitProgress());
            }
        }

        private IEnumerator UpdateUnitProgress()
        {
            while(building != null && building.QueueSize > 0)
            {
                float startTime = building.CurrentQueueStartTime;
                float endTime = startTime + building.BuildingUnit.BuildTime;

                float progress = Mathf.Clamp01((Time.time - startTime) / (endTime - startTime));

                progressBar.SetProgress(progress);
                yield return null;
            }
        }
    }
}