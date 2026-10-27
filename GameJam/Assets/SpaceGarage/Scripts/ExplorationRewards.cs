using System;
using System.Collections.Generic;

namespace SpaceGarage
{
    // Counts only successful pickups in this run, never pre-existing inventory or the bonus itself.
    public sealed class ExplorationRewards
    {
        private readonly List<string> kinds = new List<string>();
        private readonly HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, long> collectedItems = new Dictionary<string, long>(StringComparer.Ordinal);
        public IReadOnlyDictionary<string, long> CollectedItems => collectedItems;
        public long Collected { get; private set; }
        public void Record(string itemId, int amount)
        {
            if (string.IsNullOrWhiteSpace(itemId) || amount <= 0) return;
            if (seen.Add(itemId)) kinds.Add(itemId);
            collectedItems.TryGetValue(itemId, out long previous);
            collectedItems[itemId] = previous > long.MaxValue - amount ? long.MaxValue : previous + amount;
            Collected = Collected > long.MaxValue - amount ? long.MaxValue : Collected + amount;
        }
        public Dictionary<string, int> Roll(Func<int, int> randomIndex)
        {
            var result = new Dictionary<string, int>(StringComparer.Ordinal);
            if (kinds.Count == 0) return result;
            // ShipInventory accepts int quantities. Normal 30-second runs are far below this bound.
            int count = (int)Math.Min(int.MaxValue, Collected / 10);
            for (int i = 0; i < count; i++)
            {
                string id = kinds[randomIndex(kinds.Count)];
                result.TryGetValue(id, out int current);
                result[id] = current + 1;
            }
            return result;
        }
    }
}
