using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SpaceGarage
{
    // Run before Cinemachine Brain: dodge lag and boost pullback, without changing roll or target.
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(CinemachineFollow), typeof(CinemachineRotationComposer))]
    public sealed class ShipDodgeCameraLag : MonoBehaviour
    {
        [SerializeField] private ShipController ship = null;
        [SerializeField, Min(0)] private float dodgePositionDamping = 0.9f;
        [SerializeField, Min(0)] private float dodgeAimDamping = 0.6f;
        [SerializeField, Min(0.01f)] private float lagBlendInTime = 0.08f;
        [SerializeField, Min(0.01f)] private float lagRecoveryTime = 0.8f;
        [Header("Boost camera pullback")]
        [SerializeField, Min(0)] private float boostExtraDistance = 3f;
        [SerializeField, Min(0.01f)] private float boostPullbackTime = 0.35f;
        [SerializeField, Min(0.01f)] private float boostReturnTime = 0.6f;
        [Header("Right mouse camera orbit")]
        [SerializeField, Min(0)] private float orbitSensitivity = 0.15f;
        [SerializeField, Range(0, 60)] private float orbitPitchLimit = 60f;
        [SerializeField, Min(0.01f)] private float orbitReturnTime = 0.35f;
        private Vector2 orbitAngles;
        private Vector2 orbitVelocity;
        private CinemachineFollow follow;
        private CinemachineRotationComposer composer;
        private Vector3 normalPositionDamping;
        private Vector2 normalAimDamping;
        private Vector3 normalFollowOffset;
        private float boostDistance;
        private float boostDistanceVelocity;
        private float weight;
        private bool captured;

        private void OnEnable()
        {
            follow = GetComponent<CinemachineFollow>();
            composer = GetComponent<CinemachineRotationComposer>();
            if (follow == null || composer == null) return;
            normalPositionDamping = follow.TrackerSettings.PositionDamping;
            normalAimDamping = composer.Damping;
            normalFollowOffset = follow.FollowOffset;
            boostDistance = boostDistanceVelocity = 0;
            weight = 0;
            orbitAngles = orbitVelocity = Vector2.zero;
            captured = true;
        }

        private void LateUpdate()
        {
            if (!captured || follow == null || composer == null || Time.deltaTime <= 0) return;
            bool dodging = ship != null && ship.IsRolling;
            float duration = dodging ? lagBlendInTime : lagRecoveryTime;
            weight = Mathf.MoveTowards(weight, dodging ? 1 : 0, Time.deltaTime / Mathf.Max(0.01f, duration));
            follow.TrackerSettings.PositionDamping = Vector3.Lerp(normalPositionDamping,
                Vector3.Max(normalPositionDamping, Vector3.one * dodgePositionDamping), weight);
            composer.Damping = Vector2.Lerp(normalAimDamping,
                Vector2.Max(normalAimDamping, Vector2.one * dodgeAimDamping), weight);
            bool boosting = ship != null && ship.IsBoosting;
            boostDistance = Mathf.SmoothDamp(boostDistance, boosting ? boostExtraDistance : 0,
                ref boostDistanceVelocity, boosting ? boostPullbackTime : boostReturnTime,
                Mathf.Infinity, Time.deltaTime);
            UpdateOrbit();
            follow.FollowOffset = Quaternion.Euler(orbitAngles.y, orbitAngles.x, 0)
                * (normalFollowOffset + Vector3.back * boostDistance);
        }

        private void UpdateOrbit()
        {
            var mouse = Mouse.current;
            bool orbiting = Application.isFocused && ship != null && ship.IsAlive &&
                mouse != null && mouse.rightButton.isPressed;
            if (orbiting)
            {
                Vector2 delta = mouse.delta.ReadValue() * orbitSensitivity;
                orbitAngles.x = Mathf.DeltaAngle(0, orbitAngles.x + delta.x);
                orbitAngles.y = Mathf.Clamp(orbitAngles.y - delta.y, -orbitPitchLimit, orbitPitchLimit);
                orbitVelocity = Vector2.zero;
            }
            else
            {
                orbitAngles.x = Mathf.SmoothDampAngle(orbitAngles.x, 0, ref orbitVelocity.x,
                    orbitReturnTime, Mathf.Infinity, Time.deltaTime);
                orbitAngles.y = Mathf.SmoothDampAngle(orbitAngles.y, 0, ref orbitVelocity.y,
                    orbitReturnTime, Mathf.Infinity, Time.deltaTime);
                if (orbitAngles.sqrMagnitude < 0.0001f && orbitVelocity.sqrMagnitude < 0.0001f)
                    orbitAngles = orbitVelocity = Vector2.zero;
            }
        }

        private void OnDisable()
        {
            if (!captured) return;
            if (follow != null)
            {
                follow.TrackerSettings.PositionDamping = normalPositionDamping;
                follow.FollowOffset = normalFollowOffset;
            }
            if (composer != null) composer.Damping = normalAimDamping;
            captured = false;
            weight = 0;
            boostDistance = boostDistanceVelocity = 0;
            orbitAngles = orbitVelocity = Vector2.zero;
        }
    }
}
