namespace Castlebound.Gameplay.AI
{
    public interface IEnemyStaggerReceiver
    {
        bool IsActionLocked { get; }
        bool TryStagger();
    }
}
