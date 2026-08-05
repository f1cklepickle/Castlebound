namespace Castlebound.Gameplay.Combat
{
    public interface IPlayerHitReceiver
    {
        PlayerHitResult ReceiveHit(PlayerHitRequest request);
    }
}
