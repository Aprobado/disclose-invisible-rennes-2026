using Aprobado.XR.Anchors;
using Unity.Netcode;
using UnityEngine;

namespace Aprobado.Networking
{
    // On the network player prefab. The owner shares its head position, relative to the anchored environment
    // (world positions differ between headsets). The other player sees a marker above that head:
    // if the marker sits on the real person in passthrough, both the connection and the anchors are good.
    public class NetworkPlayerHead : NetworkBehaviour
    {
        [SerializeField] private Transform marker;
        [Tooltip("Height of the marker above the head")]
        [SerializeField] private float heightAboveHead = 0.2f;
        [Tooltip("Head position updates sent per second")]
        [SerializeField] private float sendRate = 10f;
        [SerializeField] private float smoothing = 12f;
        [Tooltip("Show the marker even outside of anchor edit mode (development)")]
        [SerializeField] private bool alwaysVisible;

        private readonly NetworkVariable<Vector3> headPosition = new NetworkVariable<Vector3>(
            Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private Transform head;
        private Transform referenceFrame;
        private Renderer[] markerRenderers;
        private float sendTimer;
        private bool hasPosition;

        private void Awake()
        {
            markerRenderers = marker != null ? marker.GetComponentsInChildren<Renderer>(true) : new Renderer[0];
            SetMarkerVisible(false);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            var cameraRig = FindAnyObjectByType<OVRCameraRig>();
            if (cameraRig != null) head = cameraRig.centerEyeAnchor;
            var placement = FindAnyObjectByType<PlaceEnvironmentOnFirstAnchor>();
            if (placement != null) referenceFrame = placement.EnvironmentTransform;
        }

        private void Update()
        {
            if (!IsSpawned) return;

            if (IsOwner)
            {
                SendHeadPosition();
                SetMarkerVisible(false);
                return;
            }

            if (headPosition.Value != Vector3.zero) hasPosition = true;
            SetMarkerVisible(hasPosition && (alwaysVisible || AnchorEditMode.IsActive));
            if (!hasPosition || marker == null) return;

            var target = ToWorld(headPosition.Value) + Vector3.up * heightAboveHead;
            marker.position = Vector3.Lerp(marker.position, target, 1f - Mathf.Exp(-smoothing * Time.deltaTime));
        }

        private void SendHeadPosition()
        {
            if (head == null) return;
            sendTimer -= Time.deltaTime;
            if (sendTimer > 0f) return;
            sendTimer = 1f / sendRate;

            var localPosition = referenceFrame != null ? referenceFrame.InverseTransformPoint(head.position) : head.position;
            if ((localPosition - headPosition.Value).sqrMagnitude > 0.0001f) headPosition.Value = localPosition;
        }

        private Vector3 ToWorld(Vector3 localPosition)
        {
            return referenceFrame != null ? referenceFrame.TransformPoint(localPosition) : localPosition;
        }

        private void SetMarkerVisible(bool visible)
        {
            foreach (var markerRenderer in markerRenderers)
            {
                if (markerRenderer.enabled != visible) markerRenderer.enabled = visible;
            }
        }
    }
}
