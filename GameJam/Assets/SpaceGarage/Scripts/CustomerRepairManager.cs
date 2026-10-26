using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceGarage
{
    public enum CustomerRepairState { Closed, Arriving, Docked, Departing, WaitingForNext }

    public sealed class CustomerRepairManager : MonoBehaviour
    {
        [Header("Existing IN / Stay / Out points")]
        [SerializeField] private Transform inPoint = null;
        [SerializeField] private Transform stayPoint = null;
        [SerializeField] private Transform outPoint = null;
        [SerializeField] private CustomerRepairShip customerPrefab = null;
        [Header("Random ship parts")]
        [SerializeField] private ShipPartStats[] gunPool = new ShipPartStats[0];
        [SerializeField] private ShipPartStats[] armorPlatePool = new ShipPartStats[0];
        [SerializeField] private ShipPartStats[] wingPool = new ShipPartStats[0];
        [SerializeField] private ShipPartStats[] jetPool = new ShipPartStats[0];
        [Header("Material pools assigned per part")]
        [SerializeField] private RepairMaterialPool gunMaterials = null;
        [SerializeField] private RepairMaterialPool armorPlateMaterials = null;
        [SerializeField] private RepairMaterialPool wingMaterials = null;
        [SerializeField] private RepairMaterialPool jetMaterials = null;
        [Header("Job settings")]
        [SerializeField, Range(1,100)] private int minimumDamagePercent = 20;
        [SerializeField, Range(1,100)] private int maximumDamagePercent = 90;
        [SerializeField, Min(0.01f)] private float flightSpeed = 25;
        [SerializeField, Min(0)] private float nextCustomerDelay = 1;
        [SerializeField, Min(0)] private int basePayment = 50;
        [Header("Part presentation")]
        [SerializeField, Min(0)] private float hoverLift = 0.15f;
        [SerializeField, Min(0)] private float damagedNoise = 0.06f;
        [SerializeField, Min(0)] private float hoverBrightness = 0.4f;
        public event Action Changed;
        public CustomerRepairState State { get; private set; }
        public CustomerRepairShip Customer { get; private set; }
        public Transform StayPoint => stayPoint;
        public bool IsOpen => State != CustomerRepairState.Closed;
        public bool CanSubmit => State == CustomerRepairState.Docked && Customer != null && Customer.IsFullyRepaired && !paid;
        public long QuotedPayment => Customer != null ? basePayment + Customer.RepairPayment : 0;
        private bool paid, configurationWarned;
        private float nextCustomerAt;

        [ContextMenu("Open Repair (configured pools required)")]
        public void OpenRepair() => TryOpenRepair();
        public void SubmitRepair() => TrySubmitRepair();
        public bool TryOpenRepair()
        {
            if (IsOpen) return false;
            return SpawnCustomer();
        }
        private bool SpawnCustomer()
        {
            var recipes = new[] { gunMaterials, armorPlateMaterials, wingMaterials, jetMaterials };
            var pools = new[] { gunPool, armorPlatePool, wingPool, jetPool };
            var selected = new ShipPartStats[4];
            var errors = new List<string>();
            if (inPoint == null) errors.Add("In Point: missing Transform");
            if (stayPoint == null) errors.Add("Stay Point: missing Transform");
            if (outPoint == null) errors.Add("Out Point: missing Transform");
            if (customerPrefab == null) errors.Add("Customer Prefab: missing CustomerRepairShip");
            if (ResourceManager.Instance == null) errors.Add("ResourceManager: no active instance in scene");
            for (int i = 0; i < 4; i++)
            {
                var slot = (ShipPartSlot)i;
                if (recipes[i] == null) errors.Add(slot + " Materials: missing RepairMaterialPool");
                else
                {
                    if (recipes[i].slot != slot) errors.Add(recipes[i].name + ": Slot must be " + slot);
                    if (recipes[i].ValidOptions().Count == 0)
                        errors.Add(recipes[i].name + ": no valid Materials; assign Material with Item ID and set Units At Full Damage > 0");
                }
                selected[i] = Pick(pools[i], (ShipPartSlot)i);
                if (selected[i] == null) errors.Add(slot + " Pool: needs ShipPartStats with matching Slot and Part Prefab (without ShipController/Enemy)");
            }
            if (errors.Count > 0)
            {
                if (!configurationWarned) Debug.LogWarning("Cannot open Repair:\n- " + string.Join("\n- ", errors), this);
                configurationWarned = true;
                return false;
            }
            configurationWarned = false;
            var customer = Instantiate(customerPrefab, inPoint.position, inPoint.rotation);
            try
            {
                int min = Mathf.Clamp(minimumDamagePercent, 1, 100);
                int max = Mathf.Clamp(maximumDamagePercent, min, 100);
                customer.Assemble(selected, recipes, min, max, hoverLift, damagedNoise, hoverBrightness);
            }
            catch { Destroy(customer.gameObject); throw; }
            Customer = customer; paid = false;
            customer.Docked += OnDocked; customer.Departed += OnDeparted;
            State = CustomerRepairState.Arriving;
            customer.gameObject.SetActive(true);
            customer.FlyIn(inPoint, stayPoint, flightSpeed, outPoint);
            Changed?.Invoke();
            return true;
        }

        private static ShipPartStats Pick(ShipPartStats[] pool, ShipPartSlot slot)
        {
            if (pool == null) return null;
            ShipPartStats chosen = null; int count = 0;
            foreach (var part in pool)
                if (Enemy.Valid(part, slot) && UnityEngine.Random.Range(0, ++count) == 0) chosen = part;
            return chosen;
        }
        private void OnDocked(CustomerRepairShip ship)
        { if (ship == Customer) { State = CustomerRepairState.Docked; Changed?.Invoke(); } }
        private void OnDeparted(CustomerRepairShip ship)
        {
            if (ship != Customer) return;
            ship.Docked -= OnDocked; ship.Departed -= OnDeparted;
            Customer = null;
            State = CustomerRepairState.WaitingForNext;
            nextCustomerAt = Time.time + nextCustomerDelay;
            Changed?.Invoke();
        }
        private void Update()
        {
            if (State != CustomerRepairState.WaitingForNext || Time.time < nextCustomerAt) return;
            if (!SpawnCustomer()) nextCustomerAt = Time.time + 2;
        }

        public int TryDeliver(RepairableShipPart part, ShipMaterialData material)
        {
            if (State != CustomerRepairState.Docked || Customer == null || part == null || part.Owner != Customer || ResourceManager.Instance == null) return 0;
            int count = part.Deliver(material, ResourceManager.Instance.Inventory);
            if (count > 0) Changed?.Invoke();
            return count;
        }
        public bool TrySubmitRepair()
        {
            if (!CanSubmit || ResourceManager.Instance == null) return false;
            long payment = QuotedPayment;
            State = CustomerRepairState.Departing; paid = true;
            if (payment > 0 && !ResourceManager.Instance.AddMoney(payment))
            { State = CustomerRepairState.Docked; paid = false; return false; }
            Customer.FlyOut(outPoint);
            Changed?.Invoke();
            return true;
        }
        public bool TryCloseRepair()
        {
            if (Customer != null) return false;
            State = CustomerRepairState.Closed; Changed?.Invoke(); return true;
        }
        private void OnDisable()
        {
            if (Customer != null) { Customer.Docked -= OnDocked; Customer.Departed -= OnDeparted; Destroy(Customer.gameObject); }
            Customer = null; State = CustomerRepairState.Closed;
        }
    }
}
