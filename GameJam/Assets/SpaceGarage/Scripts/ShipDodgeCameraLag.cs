using Unity.Cinemachine;
using UnityEngine;

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
            follow.FollowOffset = normalFollowOffset + Vector3.back * boostDistance;
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
        }
    }
}
