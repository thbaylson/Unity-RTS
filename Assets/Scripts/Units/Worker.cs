using UnityEngine;
using UnityEngine.AI;

namespace RTS.Units
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Worker : MonoBehaviour
    {
        [SerializeField] private Transform target;
        private NavMeshAgent agent;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        // Update is called once per frame
        void Update()
        {
            if (target != null)
            {
                agent.SetDestination(target.position);
            }
        }
    }
}