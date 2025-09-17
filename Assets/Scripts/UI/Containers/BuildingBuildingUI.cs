using RTS.UI.Components;
using RTS.Units;
using System.Collections;
using UnityEngine;

namespace RTS.UI.Containers
{
    public class BuildingBuildingUI : MonoBehaviour, IUIElement<BaseBuilding>
    {
        [SerializeField] private ProgressBar progressBar;
        [SerializeField] private UIBuildQueueButton[] unitButtons;

        private BaseBuilding building;
        private Coroutine buildCoroutine;

        public void EnableFor(BaseBuilding context)
        {
            progressBar.SetProgress(0f);
            gameObject.SetActive(true);
            building = context;
            building.OnQueueUpdated += HandleQueueUpdated;

            SetupUnitButtons();

            buildCoroutine = StartCoroutine(UpdateUnitProgress());
        }

        private void SetupUnitButtons()
        {
            int i = 0;// Leaving this outside the loop so we can continue incrementing it across multiple loops.
            for (; i < building.QueueSize; i++)
            {
                int index = i; // Capture index for the lambda. Yay encapsulation.
                unitButtons[i].EnableFor(building.Queue[i], () => building.CancelBuildingUnit(index));
            }

            for (; i < unitButtons.Length; i++)
            {
                unitButtons[i].Disable();
            }
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
            if (unitsInQueue.Length == 1 && buildCoroutine == null)
            {
                buildCoroutine = StartCoroutine(UpdateUnitProgress());
            }

            SetupUnitButtons();
        }

        private IEnumerator UpdateUnitProgress()
        {
            while (building != null && building.QueueSize > 0)
            {
                float startTime = building.CurrentQueueStartTime;
                float endTime = startTime + building.BuildingUnit.BuildTime;

                float progress = Mathf.Clamp01((Time.time - startTime) / (endTime - startTime));

                progressBar.SetProgress(progress);
                yield return null;
            }

            progressBar.SetProgress(0f);
            buildCoroutine = null;
        }
    }
}