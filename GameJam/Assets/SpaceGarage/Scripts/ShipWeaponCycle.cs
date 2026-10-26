namespace SpaceGarage
{
    public enum ShipWeaponType { Projectile, Laser, RailGun, Plasma, Getsuka }

    // Clock supplied by ShipGun, so pulse/cooldown boundaries can be tested without a scene.
    public sealed class ShipWeaponCycle
    {
        public float PulseEndsAt { get; private set; }
        public float ReadyAt { get; private set; }
        public bool IsPulsing(float now) => now < PulseEndsAt;
        public bool TryFire(float now, float activeDuration, float cooldown)
        {
            if (now < ReadyAt) return false;
            PulseEndsAt = now + activeDuration;
            ReadyAt = PulseEndsAt + cooldown;
            return true;
        }
        public void StopPulse() => PulseEndsAt = 0;
    }
}
