using RTS.Commands;
using RTS.EventBus;
using RTS.Events;
using RTS.UI.Components;
using RTS.Units;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace RTS.UI.Containers
{
    public class ActionsUI : MonoBehaviour, IUIElement<HashSet<AbstractCommandable>>
    {
        [SerializeField] private UIActionButton[] actionButtons;

        public void EnableFor(HashSet<AbstractCommandable> context)
        {
            RefreshButtons(context);
        }

        public void Disable()
        {
            foreach (UIActionButton button in actionButtons)
            {
                button.Disable();
            }
        }

        private void RefreshButtons(HashSet<AbstractCommandable> selectedUnits)
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
                    actionButtons[i].EnableFor(actionForSlot, HandleButtonClicked(actionForSlot));
                }
                else
                {
                    actionButtons[i].Disable();
                }
            }
        }

        private UnityAction HandleButtonClicked(ActionBase action)
        {
            return () => Bus<ActionSelectedEvent>.Raise(new ActionSelectedEvent(action));
        }
    }
}