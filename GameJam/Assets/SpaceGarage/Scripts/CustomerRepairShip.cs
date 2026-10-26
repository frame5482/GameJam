using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceGarage
{
    public sealed class CustomerRepairShip : MonoBehaviour
    {
        [SerializeField] private Transform gunMount = null;
        [SerializeField] private Transform armorPlateMount = null;
        [SerializeField] private Transform wingMount = null;
        [SerializeField] private Transform jetMount = null;
        public event Action<CustomerRepairShip> Docked;
        public event Action<CustomerRepairShip> Departed;
        public IReadOnlyList<RepairableShipPart> Parts => parts;
        public bool IsDocked { get; private set; }
        public bool IsFullyRepaired { get { foreach (var p in parts) if (!p.IsRepaired) return false; return parts.Count == 4; } }
        public long RepairPayment { get { long total = 0; foreach (var p in parts) total += p.Payment; return total; } }
        private readonly List<RepairableShipPart> parts = new List<RepairableShipPart>();
        private ShipBoostEffect[] engines = new ShipBoostEffect[0];
        private Transform destination;
        private float flightSpeed;
        private bool departing;

        public void Assemble(ShipPartStats[] data, RepairMaterialPool[] recipes, int minDamage, int maxDamage,
            float lift, float noise, float highlight)
        {
            if (parts.Count != 0 || data.Length != 4 || recipes.Length != 4) throw new ArgumentException("Customer assembly requires four fresh part/recipe pairs.");
            Transform[] mounts = { gunMount, armorPlateMount, wingMount, jetMount };
            for (int i = 0; i < 4; i++)
            {
                var parent = mounts[i] != null ? mounts[i] : transform;
                if (parent != transform && !parent.IsChildOf(transform)) throw new ArgumentException("Customer mount must belong to this ship.");
                var wrapper = new GameObject(data[i].slot + "_RepairPart");
                wrapper.transform.SetParent(parent, false);
                var model = Instantiate(data[i].partPrefab, wrapper.transform, false);
                model.transform.localPosition = Vector3.zero; model.transform.localRotation = Quaternion.identity;
                foreach (var gun in model.GetComponentsInChildren<ShipGun>(true)) gun.enabled = false;
                var part = wrapper.AddComponent<RepairableShipPart>();
                part.Initialize(this, data[i], model.transform, UnityEngine.Random.Range(minDamage, maxDamage + 1), recipes[i], lift, noise, highlight);
                parts.Add(part);
            }
            engines = GetComponentsInChildren<ShipBoostEffect>(true);
        }

        public void FlyIn(Transform start, Transform stay, float speed)
        {
            transform.SetPositionAndRotation(start.position, start.rotation);
            destination = stay; flightSpeed = Mathf.Max(0.01f, speed); departing = false; IsDocked = false;
            SetEngines(true);
        }
        public void FlyOut(Transform exit)
        {
            destination = exit; departing = true; IsDocked = false;
            foreach (var part in parts) part.SetHovered(false);
            SetEngines(true);
        }
        private void Update()
        {
            if (destination == null) return;
            transform.position = Vector3.MoveTowards(transform.position, destination.position, flightSpeed * Time.deltaTime);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, destination.rotation, 90 * Time.deltaTime);
            if ((transform.position - destination.position).sqrMagnitude > 0.0004f) return;
            transform.SetPositionAndRotation(destination.position, destination.rotation);
            destination = null;
            Physics.SyncTransforms();
            if (departing) { Departed?.Invoke(this); Destroy(gameObject); }
            else { IsDocked = true; SetEngines(false); Docked?.Invoke(this); }
        }
        private void SetEngines(bool running)
        { foreach (var engine in engines) if (engine != null) engine.SetFlightState(running, false); }
    }
}
