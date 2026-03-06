using RTS.Environment;

namespace RTS.Units
{
    public class Worker : AbstractUnit
    {
        public void Gather(GatherableSupply supply)
        {
            graphAgent.SetVariableValue("TargetLocation", supply.transform.position);
            graphAgent.SetVariableValue("Supply", supply);

            graphAgent.SetVariableValue("Command", UnitCommands.Gather);
        }
    }
}