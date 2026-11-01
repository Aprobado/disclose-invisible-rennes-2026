using Aprobado.XR.Anchors;
using Unity.Netcode;
using UnityEngine;

namespace Aprobado.Networking
{
    // Small light attached to the right controller, visible in anchor edit mode (right grip held):
    // red = not connected / client searching for the Host, orange = Host waiting for a client, green = both players connected.
    public class NetworkStatusLight : MonoBehaviour
    {
        [SerializeField] private Renderer lightRenderer;
        [SerializeField] private Vector3 localOffset = new Vector3(0f, 0.035f, 0.01f);
        [Tooltip("Show the light even outside of anchor edit mode (development)")]
        [SerializeField] private bool alwaysVisible;
        [SerializeField] private Color disconnectedColor = new Color(1f, 0.15f, 0.1f);
        [SerializeField] private Color waitingColor = new Color(1f, 0.6f, 0f);
        [SerializeField] private Color connectedColor = new Color(0.2f, 1f, 0.3f);

        private Material material;
        private Color currentColor;

        private void Start()
        {
            var cameraRig = FindAnyObjectByType<OVRCameraRig>();
            if (cameraRig != null && cameraRig.rightControllerAnchor != null)
            {
                transform.SetParent(cameraRig.rightControllerAnchor, false);
                transform.localPosition = localOffset;
                transform.localRotation = Quaternion.identity;
            }
            material = lightRenderer.material; // own instance to change its color
            currentColor = material.color;
        }

        private void Update()
        {
            var visible = alwaysVisible || AnchorEditMode.IsActive;
            if (lightRenderer.enabled != visible) lightRenderer.enabled = visible;
            if (!visible) return;

            var color = GetStatusColor();
            if (color == currentColor) return;
            currentColor = color;
            material.color = color;
        }

        private Color GetStatusColor()
        {
            var manager = NetworkManager.Singleton;
            if (manager == null || !manager.IsListening) return disconnectedColor;
            if (manager.IsServer) return manager.ConnectedClientsIds.Count > 1 ? connectedColor : waitingColor;
            return manager.IsConnectedClient ? connectedColor : disconnectedColor;
        }

        private void OnDestroy()
        {
            if (material != null) Destroy(material);
        }
    }
}
