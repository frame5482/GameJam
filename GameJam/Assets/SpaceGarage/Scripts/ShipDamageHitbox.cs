using UnityEngine;

namespace SpaceGarage
{
    // Only trigger colliders with this marker are considered damage targets by the gun/projectile.
    [RequireComponent(typeof(BoxCollider))]
    public sealed class ShipDamageHitbox : MonoBehaviour
    {
        private void Awake() => GetComponent<BoxCollider>().isTrigger = true;
        private void Reset() => GetComponent<BoxCollider>().isTrigger = true;
    }
}
