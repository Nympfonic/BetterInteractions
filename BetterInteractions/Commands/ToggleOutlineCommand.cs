using Arys.BetterInteractions.Components;

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
            if (_outline != null)
            {
                _outline.ToggleOutline(true);
            }
        }

        public void Undo()
        {
            if (_outline != null)
            {
                _outline.ToggleOutline(false);
            }
        }
    }
}
