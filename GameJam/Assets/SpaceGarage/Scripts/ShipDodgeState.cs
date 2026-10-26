using System;

namespace SpaceGarage
{
    // Time is supplied by the caller so cooldown and immunity share the same precise clock.
    public sealed class ShipDodgeState
    {
        public bool IsRolling { get; private set; }
        public int Direction { get; private set; }
        public double EndsAt { get; private set; }
        public double ReadyAt { get; private set; }
        private double startedAt;
        private double duration;

        public bool TryBegin(int direction, double now, double rollDuration, double cooldown)
        {
            Tick(now);
            if (direction == 0 || IsRolling || now < ReadyAt) return false;
            Direction = Math.Sign(direction);
            startedAt = now;
            duration = Math.Max(0.01, rollDuration);
            EndsAt = now + duration;
            ReadyAt = EndsAt + Math.Max(0, cooldown);
            IsRolling = true;
            return true;
        }

        public bool IsInvulnerable(double now) => IsRolling && now >= startedAt && now < EndsAt;
        public double CooldownRemaining(double now) => Math.Max(0, ReadyAt - now);
        public float Progress(double now) => (float)Math.Max(0, Math.Min(1, (now - startedAt) / Math.Max(0.01, duration)));
        public float TravelProgress(double now) => TravelProgress(now, duration);
        public float TravelProgress(double now, double travelDuration)
        {
            double interval = Math.Max(0.01, Math.Min(duration, travelDuration));
            double remaining = 1 - Math.Max(0, Math.Min(1, (now - startedAt) / interval));
            return (float)(1 - remaining * remaining * remaining);
        }
        public void Tick(double now) { if (IsRolling && now >= EndsAt) IsRolling = false; }
        public void Cancel() => IsRolling = false;
    }
}
