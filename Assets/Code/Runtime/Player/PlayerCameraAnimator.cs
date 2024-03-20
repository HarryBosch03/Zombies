using System;
using Framework.Runtime.Core;
using UnityEngine;
using Framework.Runtime.Utility;

namespace Framework.Runtime.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerCameraAnimator : MonoBehaviour
    {
        [Range(0.0f, 1.0f)]
        public float weight = 1.0f;

        [Range(0.0f, 1.0f)]
        public float fovSmoothing;
        public float fovTarget;
        public float fovActual;

        public PlayerCameraAnimatorSettings settings;

        private Vector3 smoothedPosition;
        private Quaternion smoothedRotation = Quaternion.identity;

        private PlayerController controller;
        private Camera mainCamera;
        private float distance;

        public PlayerMovement biped => controller.biped;
        public float fovOverride { get; set; } = 50.0f;
        public float fovOverrideBlend { get; set; }
        public float functionalZoom { get; set; } = 1.0f;
        public float effectZoom { get; set; } = 1.0f;
        public Quaternion rotationOffset { get; set; } = Quaternion.identity;

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            mainCamera = Camera.main;
        }

        private void LateUpdate()
        {
            UpdateState();
            var pose = CompilePose();
            UpdateCamera(pose, fovActual);
        }

        private float CalculateFov()
        {
            var angle = Mathf.Lerp(settings.fieldOfView, fovOverride, fovOverrideBlend) * 0.5f * Mathf.Deg2Rad;
            var tangent = Mathf.Tan(angle);

            var zoom = functionalZoom * Mathf.Lerp(1.0f, effectZoom, settings.fovEffects);
            angle = Mathf.Atan(tangent / zoom);

            return angle * 2.0f * Mathf.Rad2Deg;
        }

        private void FixedUpdate()
        {
            functionalZoom = 1.0f;
            effectZoom = 1.0f;

            fovTarget = CalculateFov();
            fovActual += (fovTarget - fovActual) * (1.0f - fovSmoothing);

            CalculateZoomEffects();
        }

        private void CalculateZoomEffects()
        {
            if (biped.running) effectZoom *= settings.runZoom;
        }

        private void UpdateState()
        {
            var speed = biped.groundSpeed;
            distance += speed * Time.deltaTime;
        }

        private void UpdateCamera(Pose pose, float fov)
        {
            var view = biped.view;

            var position = view.position;
            var rotation = view.rotation;

            position += rotation * Vector3.Lerp(Vector3.zero, pose.position, weight);
            rotation *= Quaternion.Lerp(Quaternion.identity, pose.rotation, weight);

            rotation *= rotationOffset;
            
            smoothedPosition = Vector3.Lerp(position, smoothedPosition, Time.deltaTime * settings.poseSmoothing);
            smoothedRotation = Quaternion.Slerp(rotation, smoothedRotation, Time.deltaTime * settings.poseSmoothing);

            var cam = mainCamera.transform;
            cam.position = smoothedPosition;
            cam.rotation = smoothedRotation;
            mainCamera.fieldOfView = fov;
        }

        private Pose CompilePose()
        {
            var pose = settings.idlePose.CreatePose(distance);
            if (biped.isOnGround) pose = settings.runPose.Apply(pose, distance, biped.normalizedGroundSpeed);
            return pose;
        }

        [Serializable]
        public class SwayPose
        {
            [SerializeField] private float baseFrequency = 1.0f;
            [SerializeField] private float baseAmplitude = 1.0f;

            [SerializeField] private Vector2 translationFrequency;
            [SerializeField] private Vector2 translationAmplitude;

            [SerializeField] private Vector3 canterFrequency;
            [SerializeField] private Vector3 canterAmplitude;

            public Pose CreatePose(float distance) => Apply(default, distance, 1.0f);

            public Pose Apply(Pose pose, float distance, float weight)
            {
                var t = distance * baseFrequency;
                var rawPose = new Pose();

                rawPose.position = new Vector3
                {
                    x = Mathf.Sin(t * Mathf.PI * translationFrequency.x) * translationAmplitude.x,
                    y = Mathf.Sin(t * Mathf.PI * translationFrequency.y) * translationAmplitude.y,
                    z = 0.0f,
                } * baseAmplitude;

                rawPose.rotation = (new Vector3
                {
                    x = Mathf.Sin(t * Mathf.PI * canterFrequency.x) * canterAmplitude.x,
                    y = Mathf.Sin(t * Mathf.PI * canterFrequency.y) * canterAmplitude.y,
                    z = Mathf.Sin(t * Mathf.PI * canterFrequency.z) * canterAmplitude.z,
                } * baseAmplitude).Euler();

                pose.position = Vector3.Lerp(pose.position, rawPose.position, weight);
                pose.rotation = Quaternion.Slerp(pose.rotation, rawPose.rotation, weight);

                return pose;
            }
        }
    }
}