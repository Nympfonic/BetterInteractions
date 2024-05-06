namespace Arys.BetterInteractions.Interfaces
{
    public interface ICommand
    {
        void Execute();
        void Undo();
    }
}
