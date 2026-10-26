using System;

namespace SpaceGarage
{
    public enum EnemyFirePhase { Idle, Warning, Firing, Paused }

    public sealed class EnemyFireCycle
    {
        public EnemyFirePhase Phase { get; private set; }
        public double NextBurstAt { get; private set; } = -1;
        private double burstEndsAt = -1;
        private double warningStarted = -1;

        public EnemyFirePhase Tick(double now, bool canEngage, double warning, double burstDuration, double pauseDuration)
        {
            warning = Math.Max(0, warning);
            pauseDuration = Math.Max(0, pauseDuration);
            FinishElapsedBurst(now, pauseDuration);
            if (!canEngage) { Interrupt(now, pauseDuration); return Phase; }
            if (Phase == EnemyFirePhase.Firing) return Phase;
            // Show the next warning during the end of the pause, without extending a normal full pause.
            if (NextBurstAt >= 0 && now < NextBurstAt - Math.Min(warning, pauseDuration))
                return Phase = EnemyFirePhase.Paused;
            if (warningStarted < 0) warningStarted = now;
            if (now < NextBurstAt || now - warningStarted < warning)
                return Phase = EnemyFirePhase.Warning;
            burstEndsAt = now + Math.Max(0.01, burstDuration);
            return Phase = EnemyFirePhase.Firing;
        }

        private void FinishElapsedBurst(double now, double pauseDuration)
        {
            if (Phase != EnemyFirePhase.Firing || now < burstEndsAt) return;
            NextBurstAt = burstEndsAt + Math.Max(0, pauseDuration);
            burstEndsAt = -1;
            warningStarted = -1;
            Phase = EnemyFirePhase.Paused;
        }

        public void Interrupt(double now, double pauseDuration)
        {
            FinishElapsedBurst(now, pauseDuration);
            if (Phase == EnemyFirePhase.Firing) NextBurstAt = now + Math.Max(0, pauseDuration);
            burstEndsAt = -1;
            warningStarted = -1;
            Phase = now < NextBurstAt ? EnemyFirePhase.Paused : EnemyFirePhase.Idle;
        }

        public void Reset()
        {
            Phase = EnemyFirePhase.Idle;
            NextBurstAt = burstEndsAt = warningStarted = -1;
        }
    }
}
