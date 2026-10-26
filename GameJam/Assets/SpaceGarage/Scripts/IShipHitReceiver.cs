using UnityEngine;

namespace SpaceGarage
{
    public enum ShipHitSource { Unknown, Player, Enemy }

    public interface IShipHitReceiver : IShipDamageable
    {
        void TakeHit(float amount, ShipHitSource source, Vector3 point, Vector3 normal);
    }
}
