using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;

namespace RTS.Units
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Worker : MonoBehaviour, ISelectable, IMoveable
    {
        [SerializeField] private DecalProjector decalProjector;
        private NavMeshAgent agent;

        public void Deselect()
        {
            if(decalProjector != null)
            {
                decalProjector.gameObject.SetActive(false);
            }
        }

        public void MoveTo(Vector3 position)
        {
            agent.SetDestination(position);
        }

        public void Select()
        {
            if (decalProjector != null)
            {
                decalProjector.gameObject.SetActive(true);
            }
        }

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }
    }
}