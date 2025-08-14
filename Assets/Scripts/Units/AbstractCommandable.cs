using RTS.EventBus;
using RTS.Events;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace RTS.Units
{
    public abstract class AbstractCommandable : MonoBehaviour, ISelectable
    {
        [field: SerializeField] public int MaxHealth { get; private set; }
        [field: SerializeField] public int CurrentHealth { get; private set; }
        
        [SerializeField] private UnitSO UnitSO;
        [SerializeField] private DecalProjector decalProjector;

        // Setting properties from the SO is best done in Start.
        protected virtual void Start()
        {
            if(UnitSO != null)
            {
                CurrentHealth = UnitSO.Health;
                MaxHealth = UnitSO.Health;
            }
            else
            {
                Debug.LogWarning("UnitSO is not assigned in " + gameObject.name);
            }
        }

        public void Deselect()
        {
            if (decalProjector != null)
            {
                decalProjector.gameObject.SetActive(false);
            }

            Bus<UnitDeselectedEvent>.Raise(new(this));
        }

        public void Select()
        {
            if (decalProjector != null)
            {
                decalProjector.gameObject.SetActive(true);
            }

            Bus<UnitSelectedEvent>.Raise(new(this));
        }
    }
}