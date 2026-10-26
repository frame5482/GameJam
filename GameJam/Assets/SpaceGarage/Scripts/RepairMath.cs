using System;

namespace SpaceGarage
{
    public static class RepairMath
    {
        public static int RequiredUnits(int unitsAtFullDamage, int damagePercent)
        {
            if (unitsAtFullDamage <= 0 || damagePercent <= 0) return 0;
            return (int)Math.Min(int.MaxValue, ((long)unitsAtFullDamage * Math.Min(100, damagePercent) + 99) / 100);
        }
        public static int DeliveryAmount(int remaining, int available) => Math.Max(0, Math.Min(remaining, available));
    }
}
