using System;

namespace SpaceGarage
{
    public static class EnemyCombatMotion
    {
        // Positive approaches the player, negative backs away while keeping the nose aimed at them.
        public static float FrontSpeedFactor(float distance, float keepDistance, float responseDistance)
        {
            double factor = (distance - Math.Max(0, keepDistance)) / Math.Max(0.01, responseDistance);
            return (float)Math.Max(-1, Math.Min(1, factor));
        }
    }
}
