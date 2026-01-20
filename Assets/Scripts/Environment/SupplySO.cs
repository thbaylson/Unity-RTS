using UnityEngine;

namespace RTS.Environment
{
    [CreateAssetMenu(fileName = "Supply", menuName = "Supply", order = 5)]
    public class SupplySO : ScriptableObject
    {
        [field: SerializeField] public int MaxAmount { get; private set; } = 1500;
        [field: SerializeField] public int AmountPerGather { get; private set; } = 8;
        [field: SerializeField] public float BaseGatherTime { get; private set; } = 1.5f;
    }
}
