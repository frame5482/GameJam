using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceGarage
{
    [Serializable]
    public sealed class RepairRequirement
    {
        public ShipMaterialData material;
        public int required;
        public int delivered;
        public int Remaining => Math.Max(0, required - delivered);
    }

    public sealed class RepairableShipPart : MonoBehaviour
    {
        public CustomerRepairShip Owner { get; private set; }
        public ShipPartSlot Slot { get; private set; }
        public int OriginalDamagePercent { get; private set; }
        public IReadOnlyList<RepairRequirement> Requirements => requirements;
        public long Payment { get; private set; }
        public long RemainingUnits { get { long sum = 0; foreach (var r in requirements) sum += r.Remaining; return sum; } }
        public long TotalUnits { get { long sum = 0; foreach (var r in requirements) sum += r.required; return sum; } }
        public bool IsRepaired => RemainingUnits == 0;
        public int DamagePercent => TotalUnits <= 0 ? 0 : Mathf.CeilToInt(OriginalDamagePercent * (RemainingUnits / (float)TotalUnits));
        private readonly List<RepairRequirement> requirements = new List<RepairRequirement>();
        private readonly List<Tint> tints = new List<Tint>();
        private Transform visuals;
        private Vector3 restPosition;
        private Quaternion restRotation;
        private float noiseSeed, lift;
        private bool hovered;
        private float hoverLift, noiseAmount, hoverBrightness;

        private sealed class Tint
        {
            public Renderer renderer;
            public int slot;
            public MaterialPropertyBlock original, working;
            public Color color, emission;
        }

        public void Initialize(CustomerRepairShip owner, ShipPartStats data, Transform model, int damage,
            RepairMaterialPool pool, float liftHeight, float noise, float highlight)
        {
            Owner = owner; Slot = data.slot; visuals = model;
            OriginalDamagePercent = Mathf.Clamp(damage, 0, 100);
            restPosition = model.localPosition; restRotation = model.localRotation;
            hoverLift = liftHeight; noiseAmount = noise; hoverBrightness = highlight;
            noiseSeed = UnityEngine.Random.value * 500;
            var options = pool.ValidOptions();
            int min = Mathf.Clamp(pool.minimumMaterialTypes, 1, options.Count);
            int max = Mathf.Clamp(pool.maximumMaterialTypes, min, options.Count);
            int count = UnityEngine.Random.Range(min, max + 1);
            for (int i = 0; i < count; i++)
            {
                int pick = UnityEngine.Random.Range(0, options.Count);
                var option = options[pick]; options.RemoveAt(pick);
                int units = RepairMath.RequiredUnits(option.unitsAtFullDamage, OriginalDamagePercent);
                if (units <= 0) continue;
                requirements.Add(new RepairRequirement { material = option.material, required = units });
                Payment += (long)units * Math.Max(0, option.paymentPerUnit);
                if (ResourceManager.Instance != null) ResourceManager.Instance.RegisterMaterial(option.material);
            }
            foreach (var renderer in model.GetComponentsInChildren<Renderer>(true))
            {
                if (!(renderer is MeshRenderer) && !(renderer is SkinnedMeshRenderer)) continue;
                var materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    var material = materials[i]; if (material == null) continue;
                    var original = new MaterialPropertyBlock(); var working = new MaterialPropertyBlock();
                    renderer.GetPropertyBlock(original, i); renderer.GetPropertyBlock(working, i);
                    Color color = material.HasProperty("_BaseColor") ? material.GetColor("_BaseColor") :
                        material.HasProperty("_Color") ? material.GetColor("_Color") : Color.white;
                    color = ShipRandomColors.Sample(color.a);
                    tints.Add(new Tint { renderer = renderer, slot = i, original = original, working = working,
                        color = color, emission = material.HasProperty("_EmissionColor") ? material.GetColor("_EmissionColor") : Color.black });
                }
            }
            EnsurePickCollider();
        }

        private void EnsurePickCollider()
        {
            if (GetComponentInChildren<Collider>() != null) return;
            bool found = false; var bounds = new Bounds();
            foreach (var renderer in visuals.GetComponentsInChildren<Renderer>())
            {
                if (!(renderer is MeshRenderer) && !(renderer is SkinnedMeshRenderer)) continue;
                var world = renderer.bounds;
                for (int x = -1; x <= 1; x += 2) for (int y = -1; y <= 1; y += 2) for (int z = -1; z <= 1; z += 2)
                {
                    Vector3 point = transform.InverseTransformPoint(world.center + Vector3.Scale(world.extents, new Vector3(x,y,z)));
                    if (!found) { bounds = new Bounds(point, Vector3.zero); found = true; } else bounds.Encapsulate(point);
                }
            }
            if (!found) return;
            var box = gameObject.AddComponent<BoxCollider>(); box.center = bounds.center;
            box.size = Vector3.Max(bounds.size, Vector3.one * 0.15f) + Vector3.one * 0.05f;
            box.isTrigger = true;
        }

        public int Deliver(ShipMaterialData material, ShipInventory inventory)
        {
            if (material == null || inventory == null) return 0;
            foreach (var requirement in requirements)
                if (requirement.material.itemId == material.itemId)
                {
                    int amount = RepairMath.DeliveryAmount(requirement.Remaining, inventory.Count(material.itemId));
                    if (amount <= 0 || !inventory.TryRemove(material.itemId, amount)) return 0;
                    requirement.delivered += amount;
                    return amount;
                }
            return 0;
        }

        public void SetHovered(bool value) => hovered = value;
        private void LateUpdate()
        {
            if (visuals == null) return;
            lift = Mathf.MoveTowards(lift, hovered ? hoverLift : 0, Time.deltaTime * 0.8f);
            float damaged = DamagePercent / 100f;
            Vector3 jitter = new Vector3(Mathf.PerlinNoise(noiseSeed, Time.time * 4) - 0.5f,
                Mathf.PerlinNoise(noiseSeed + 9, Time.time * 5) - 0.5f, 0) * noiseAmount * damaged;
            Vector3 hoverOffset = visuals.parent != null ? visuals.parent.InverseTransformVector(Vector3.up * lift) : Vector3.up * lift;
            visuals.localPosition = restPosition + hoverOffset + jitter;
            visuals.localRotation = restRotation * Quaternion.Euler(jitter.y * 20, 0, jitter.x * 20);
            foreach (var tint in tints)
            {
                if (tint.renderer == null) continue;
                Color color = tint.color * (Mathf.Lerp(1, 0.45f, damaged) + (hovered ? hoverBrightness : 0));
                color.a = tint.color.a;
                tint.working.SetColor("_BaseColor", color); tint.working.SetColor("_Color", color);
                tint.working.SetColor("_EmissionColor", tint.emission + (hovered ? new Color(0.05f,0.3f,0.4f) * hoverBrightness : Color.black));
                tint.renderer.SetPropertyBlock(tint.working, tint.slot);
            }
        }
        private void OnDestroy()
        {
            foreach (var tint in tints) if (tint.renderer != null) tint.renderer.SetPropertyBlock(tint.original, tint.slot);
        }
    }
}
