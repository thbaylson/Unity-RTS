using RTS.Units;
using UnityEngine;

namespace RTS.Commands
{
    [CreateAssetMenu(fileName = "Move Action", menuName = "AI/Actions/Move", order = 100)]
    public class MoveCommand : ActionBase
    {
        [SerializeField] private float unitSpacingMultiplier = 2f;
        [SerializeField] private float layerSpacingMultiplier = 3.5f;

        private int unitsOnLayer = 0;
        private int maxUnitsOnLayer = 1;
        private float circleRadius = 0f;
        private float radialOffset = 0f;

        public override bool CanHandle(CommandContext ctx)
        {
            return ctx.Commandable is AbstractUnit;
        }

        // When we have multiple units moving to the same location, we need to prevent
        // the units from bumping into each other and pushing each other around, ie "Unit Dancing."
        // To do this, we're going to make concentric rings with radi based on how many units
        // need to be placed. We'll use degrees on the Unit Circle to figure out how to equally space 
        // the units on a given layer.
        public override void Handle(CommandContext ctx)
        {
            AbstractUnit unit = (AbstractUnit)ctx.Commandable;

            // We only want to reset these on the first unit. This will perserve the functionality of moving multiple units.
            if (ctx.UnitIndex == 0)
            {
                unitsOnLayer = 0;
                maxUnitsOnLayer = 1;
                circleRadius = 0f;
                radialOffset = 0f;
            }

            // Find where the unit should be on its layer of the concentric circles.
            Vector3 targetPosition = new(
                ctx.Hit.point.x + circleRadius * Mathf.Cos(radialOffset * unitsOnLayer),
                ctx.Hit.point.y,
                ctx.Hit.point.z + circleRadius * Mathf.Sin(radialOffset * unitsOnLayer)
            );

            unit.MoveTo(targetPosition);
            unitsOnLayer++;

            if (unitsOnLayer >= maxUnitsOnLayer)
            {
                // Once we fill up a layer, calculate the next layer's radius and reset the count.
                circleRadius += unit.AgentRadius * layerSpacingMultiplier;
                unitsOnLayer = 0;

                // The circumfrence of the circle divided by (the radius of our units * unit spacing offset).
                maxUnitsOnLayer = Mathf.FloorToInt(2 * Mathf.PI * circleRadius / (unit.AgentRadius * unitSpacingMultiplier));
                // 2 * Mathf.PI is the radians of a full circle, then divide by max units to get the radial offset.
                // TODO: Units on the last layer are not spaced evenly. This formula assumes we'll always have the max number
                // on each layer. We could space them out better if we could predict how many units will be on the last layer.
                radialOffset = 2 * Mathf.PI / maxUnitsOnLayer;
            }
        }
    }
}