using UnityEngine;

namespace SpaceGarage
{
    // Shared by player and enemies. No AI decisions or UI are made here.
    public static class ShipPartTraits
    {
        public static ShipStats Apply(ShipStats total, ShipPartStats armor, ShipPartStats wing, ShipPartStats jet, bool alternate)
        {
            ApplyPart(ref total, armor, alternate);
            ApplyPart(ref total, wing, alternate);
            ApplyPart(ref total, jet, alternate);
            total.speed = Mathf.Max(0, total.speed);
            total.handling = Mathf.Max(0, total.handling);
            total.health = Mathf.Max(1, total.health);
            return total;
        }

        private static void ApplyPart(ref ShipStats total, ShipPartStats part, bool alternate)
        {
            if (part == null) return;
            bool mode = alternate && part.switchableMode;
            total.speed *= mode ? part.alternateSpeedMultiplier : part.speedMultiplier;
            total.handling *= mode ? part.alternateHandlingMultiplier : part.handlingMultiplier;
            total.health *= part.healthMultiplier;
            if (mode) total.defense += part.alternateDefense;
        }
    }

    public sealed class ShipBarrierState
    {
        public float Current { get; private set; }
        public float Capacity => part != null ? Mathf.Max(0, part.barrierCapacity) : 0;
        private ShipPartStats part;
        private GameObject visual;
        private float recoverAt;

        public void Configure(ShipPartStats armor, Transform owner)
        {
            Dispose();
            part = armor;
            Current = Capacity;
            recoverAt = 0;
            if (Capacity > 0 && part.barrierPrefab != null)
            {
                visual = Object.Instantiate(part.barrierPrefab, owner, false);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Quaternion.identity;
                visual.transform.localScale *= part.barrierVisualScale;
                // This is a visual shield: damage is intercepted by the ship, not by extra colliders.
                foreach (var collider in visual.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
            }
        }

        public float Absorb(float damage)
        {
            if (damage <= 0) return 0;
            float absorbed = Mathf.Min(Current, damage);
            Current -= absorbed;
            if (part != null) recoverAt = Time.time + part.barrierRecoveryDelay;
            SetVisible(Current > 0);
            return damage - absorbed;
        }

        public void Tick(bool alive, float delta)
        {
            if (alive && part != null && Time.time >= recoverAt)
                Current = Mathf.Min(Capacity, Current + part.barrierRecoveryPerSecond * delta);
            SetVisible(alive && Current > 0);
        }

        public void SetVisible(bool active)
        {
            if (visual != null && visual.activeSelf != active) visual.SetActive(active);
        }

        public void Dispose()
        {
            if (visual != null) { visual.SetActive(false); Object.Destroy(visual); }
            visual = null;
        }
    }
}
