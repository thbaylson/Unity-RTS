using RTS.Units;
using System.Linq;
using UnityEngine;

namespace RTS.Commands
{
    [CreateAssetMenu(fileName = "Move Action", menuName = "AI/Actions/Move", order = 100)]
    public class MoveCommand : ScriptableObject, ICommand
    {
        public bool CanHandle(AbstractCommandable commandable, RaycastHit hit)
        {
            return commandable is IMoveable;
        }
        public void Handle(AbstractCommandable commandable, RaycastHit hit)
        {
            if(commandable is IMoveable moveable)
            {
                moveable.MoveTo(hit.point);
            }
        }
    }
}