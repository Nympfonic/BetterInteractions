using Arys.BetterInteractions.Components;
using Arys.BetterInteractions.Interfaces;

namespace Arys.BetterInteractions.Commands
{
    internal class ToggleOutlineCommand : ICommand
    {
        private readonly BetterInteractionsOutline _outline;

        internal ToggleOutlineCommand(BetterInteractionsOutline outline)
        {
            _outline = outline;
        }

        public void Execute()
        {
            _outline.ToggleOutline(true);
        }

        public void Undo()
        {
            _outline.ToggleOutline(false);
        }
    }
}
