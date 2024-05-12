using System.Collections.Generic;
using Arys.BetterInteractions.Commands;

namespace Arys.BetterInteractions.Controllers
{
    public class OutlineController
    {
        private readonly Queue<ICommand> _commandList;

        public OutlineController()
        {
            _commandList = new Queue<ICommand>(2);
        }

        public void AddCommand(ICommand newCommand)
        {
            UndoCommand();
            newCommand.Execute();
            _commandList.Enqueue(newCommand);
        }

        public void UndoCommand()
        {
            if (_commandList.Count > 0)
            {
                ICommand prevCommand = _commandList.Dequeue();
                prevCommand.Undo();
            }
        }

        public void ClearCommandList()
        {
            _commandList.Clear();
        }
    }
}
