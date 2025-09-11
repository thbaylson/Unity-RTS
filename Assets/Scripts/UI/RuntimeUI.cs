using RTS.EventBus;
using RTS.Events;
using RTS.UI.Containers;
using RTS.Units;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RTS.UI
{
    public class  RuntimeUI : MonoBehaviour
    {
        [SerializeField] private ActionsUI actionsUI;
        [SerializeField] private BuildingBuildingUI buildingBuildingUI;

        private HashSet<AbstractCommandable> selectedUnits = new(12);

        private void Awake()
        {
            Bus<UnitSelectedEvent>.OnEvent += HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent += HandleUnitDeselected;
        }

        private void Start()
        {
            actionsUI.Disable();
            buildingBuildingUI.Disable();
        }

        private void OnDestroy()
        {
            Bus<UnitDeselectedEvent>.OnEvent -= HandleUnitDeselected;
            Bus<UnitSelectedEvent>.OnEvent -= HandleUnitSelected;
        }

        private void HandleUnitSelected(UnitSelectedEvent evt)
        {
            if(evt.Unit is AbstractCommandable commandable)
            {
                selectedUnits.Add(commandable);
                actionsUI.EnableFor(selectedUnits);
            }

            // There is no UI for displaying multiple build queues, so only show if exactly 1 is selected.
            if(selectedUnits.Count == 1 && evt.Unit is BaseBuilding building)
            {
                buildingBuildingUI.EnableFor(building);
            }
        }

        private void HandleUnitDeselected(UnitDeselectedEvent evt)
        {
            if (evt.Unit is AbstractCommandable commandable)
            {
                selectedUnits.Remove(commandable);

                if(selectedUnits.Count > 0)
                {
                    actionsUI.EnableFor(selectedUnits);

                    if(selectedUnits.Count == 1 && selectedUnits.First() is BaseBuilding building)
                    {
                        buildingBuildingUI.EnableFor(building);
                    }
                    else
                    {
                        buildingBuildingUI.Disable();
                    }
                }
                else
                {
                    actionsUI.Disable();
                    buildingBuildingUI.Disable();
                }
            }
        }
    }
}