using RTS.Units;
using UnityEngine;

namespace RTS.Commands
{
    [CreateAssetMenu(fileName = "Stop Action", menuName = "AI/Commands/Stop", order = 101)]
    public class StopCommand : ActionBase
    {
        public override bool CanHandle(CommandContext ctx)
        {
            return ctx.Commandable is AbstractUnit;
        }

        public override void Handle(CommandContext ctx)
        {
            ((AbstractUnit)ctx.Commandable).Stop();
        }
    }
}