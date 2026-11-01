using System.Collections.Generic;
using UnityEngine;

namespace Aprobado.XR.Anchors
{
    public class PlaceEnvironmentOnFirstAnchor : MonoBehaviour
    {
        [SerializeField] private Transform environmentTransform;
        [SerializeField] private Transform anchorDebugElements;

        // The anchored frame shared by both players (each headset places it on its own anchor)
        public Transform EnvironmentTransform => environmentTransform;

        private OVRCameraRig cameraRig;
        private OVRSpatialAnchor anchorUsed;
        private float refreshTime = 1.5f;
        private bool overrideElevation;
        private float elevation;

        private Pose targetPose;
        private Vector3 positionDampVelocity;
        private float yawDampVelocity;
        
        private void Awake()
        {
            cameraRig = FindAnyObjectByType<OVRCameraRig>();
        }

        private void Update()
        {
            if (Application.isEditor) return;

            if (anchorUsed == null && cameraRig.rightControllerAnchor != null && AnchorEditMode.IsActive)
            {
                var pose = GetFlattenedPose(cameraRig.rightControllerAnchor);
                anchorDebugElements.position = pose.position;
                anchorDebugElements.rotation = pose.rotation;
            }
            else if (anchorUsed != null)
            {
                refreshTime -= Time.deltaTime;
                if (refreshTime < 0f)
                {
                    SetTargetPoseFromAnchor(anchorUsed.transform);
                    // SetEnvironmentPoseFromAnchorTransform(anchorUsed.transform);
                    refreshTime = 1.5f;
                }
                SmoothDampToTarget();
            }
            else
            {
                positionDampVelocity = Vector3.zero;
                yawDampVelocity = 0f;
            }
        }

        public void OnAnchorCreateCompleted(OVRSpatialAnchor anchor, OVRSpatialAnchor.OperationResult result)
        {
            if (result != OVRSpatialAnchor.OperationResult.Success)
            {
                Debug.LogError($"Anchor creation failed with : {result}");
                return;
            }

            anchorUsed = anchor;
            SetTargetPoseFromAnchor(anchor.transform);
        }

        public void OnAnchorsLoadCompleted(List<OVRSpatialAnchor> anchors)
        {
            if (anchors.Count > 0)
            {
                anchorUsed = anchors[0];
                SetTargetPoseFromAnchor(anchors[0].transform);
            }
            else
            {
                anchorUsed = null;
            }
        }

        public void OnAnchorEraseAll(OVRSpatialAnchor.OperationResult result)
        {
            anchorUsed = null;
        }

        public void ElevationOverride(float _elevation)
        {
            overrideElevation = true;
            elevation = _elevation;
        }

        private void SetTargetPoseFromAnchor(Transform anchor)
        {
            targetPose = GetFlattenedPose(anchor);
        }

        private void SmoothDampToTarget()
        {
            // jump if distance is large. Otherwise, smooth towards target
            var dist = Vector3.Distance(environmentTransform.position, targetPose.position);
            if (dist > 1f)
            {
                environmentTransform.position = targetPose.position;
                anchorDebugElements.position = targetPose.position;
                positionDampVelocity = Vector3.zero;
            }
            else
            {
                var pos = Vector3.SmoothDamp(environmentTransform.position, targetPose.position,
                    ref positionDampVelocity, 1f, 5f);
                environmentTransform.position = pos;
                anchorDebugElements.position = pos;
            }

            // jump if angle is large. Otherwise, smooth towards target
            var angle = Vector3.Angle(environmentTransform.forward, targetPose.forward);
            if (angle > 30)
            {
                environmentTransform.rotation = targetPose.rotation;
                anchorDebugElements.rotation = targetPose.rotation;
                yawDampVelocity = 0f;
            }
            else
            {
                // The pose is flattened (yaw only), so damping the Y angle is enough and avoids quaternion drift
                var currentYaw = environmentTransform.eulerAngles.y;
                var targetYaw = targetPose.rotation.eulerAngles.y;
                var yaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref yawDampVelocity, 1f, 30f);
                environmentTransform.rotation = Quaternion.Euler(0f, yaw, 0f);
                anchorDebugElements.rotation = Quaternion.Euler(0f, yaw, 0f);
            }
        }

        private Pose GetFlattenedPose(Transform from)
        {
            var position = from.position;
            position.y = overrideElevation ? elevation : 0f;
            var forward = from.forward;
            forward.y = 0f;
            forward.Normalize();
            var rotation = Quaternion.LookRotation(forward, Vector3.up);
            return new Pose(position, rotation);
        }
    }
}