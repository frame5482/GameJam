using UnityEngine;

namespace SpaceGarage
{
    // Optional contract. Prefabs without this interface can use their own scripts/settings.
    public interface IShipProjectile
    {
        void Initialize(Transform owner, float damage, float speed, float lifetime);
    }
}
