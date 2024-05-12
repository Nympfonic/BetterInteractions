namespace Arys.BetterInteractions.Commands
{
    public interface ICommand
    {
        void Execute();
        void Undo();
    }
}
