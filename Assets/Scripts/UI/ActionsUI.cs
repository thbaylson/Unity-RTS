using RTS.Commands;
using RTS.EventBus;
using RTS.Events;
using RTS.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RTS.UI
{
    public class ActionsUI : MonoBehaviour
    {
        [SerializeField] private UIActionButton[] actionButtons;
        private HashSet<AbstractCommandable> selectedUnits = new(12);

        private void Awake()
        {
            Bus<UnitSelectedEvent>.OnEvent += HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent += HandleUnitDeselected;

            foreach(UIActionButton button in actionButtons)
            {
                button.SetIcon(null);
            }
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
                RefreshButtons();
            }
        }

        private void HandleUnitDeselected(UnitDeselectedEvent evt)
        {
            if(evt.Unit is AbstractCommandable commandable)
            {
                selectedUnits.Remove(commandable);
                RefreshButtons();
            }
        }

        private void RefreshButtons()
        {
            HashSet<ActionBase> availableCommands = new(9);
            foreach (AbstractCommandable commandable in selectedUnits)
            {
                // Ensure we only add unique commands. Commands are ScriptableObjects, so each instance of a command is the same instance.
                availableCommands.UnionWith(commandable.AvailableCommands);
            }

            for(int i = 0; i < actionButtons.Length; i++)
            {
                ActionBase actionForSlot = availableCommands.Where(action => action.Slot == i).FirstOrDefault();
                // Normally I would do null propagation here to condense the logic, eg: (actionForSlot?.Icon ?? null),
                // but Unity recommends against using null propagation and null coalescing operators with Unity Objects.
                // This stems from how they override the == operator and how they do null checks.
                if (actionForSlot != null)
                {
                    actionButtons[i].SetIcon(actionForSlot.Icon);
                }
                else
                {
                    actionButtons[i].SetIcon(null);
                }
            }
        }
    }
}