using RTS.Environment;
using RTS.Units;
using UnityEngine;

namespace RTS.Commands
{
    [CreateAssetMenu(fileName = "Gather Action", menuName = "AI/Commands/Gather", order = 105)]
    public class GatherCommand : ActionBase
    {
        //
        public override bool CanHandle(CommandContext ctx)
        {
            // We can only gather if the commandable is a worker, the raycast hit something,
            // and the thing it hit is a gatherable supply.
            return ctx.Commandable is Worker
                && ctx.Hit.collider != null
                && ctx.Hit.collider.gameObject.TryGetComponent(out GatherableSupply _);
        }

        public override void Handle(CommandContext ctx)
        {
            (ctx.Commandable as Worker).Gather(ctx.Hit.collider.gameObject.GetComponent<GatherableSupply>());
        }
    }
}