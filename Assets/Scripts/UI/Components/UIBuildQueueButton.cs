using RTS.Units;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RTS.UI.Components
{
    [RequireComponent(typeof(Button))]
    public class UIBuildQueueButton : MonoBehaviour, IUIElement<UnitSO, UnityAction>
    {
        [SerializeField] private Image icon;

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            Disable();
        }

        public void EnableFor(UnitSO context, UnityAction callback)
        {
            button.onClick.RemoveAllListeners();

            icon.gameObject.SetActive(true);
            icon.sprite = context.Icon;

            button.interactable = true;
            button.onClick.AddListener(callback);
        }

        public void Disable()
        {
            icon.gameObject.SetActive(false);

            button.interactable = false;
            button.onClick.RemoveAllListeners();
        }
    }
}