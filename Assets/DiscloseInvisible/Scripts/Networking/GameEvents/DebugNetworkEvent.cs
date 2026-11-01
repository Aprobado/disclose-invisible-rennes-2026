using UnityEngine;

namespace Aprobado.Networking
{
    public class DebugNetworkEvent : MonoBehaviour
    {
        private void OnEnable()
        {
            NetworkGameEvents.OnDebugNetworkingReceived += NetworkGameEvents_OnDebugNetworkingReceived;
        }

        private void OnDisable()
        {
            NetworkGameEvents.OnDebugNetworkingReceived -= NetworkGameEvents_OnDebugNetworkingReceived;
        }

        private void NetworkGameEvents_OnDebugNetworkingReceived()
        {
            Debug.Log($"DebugNetworking Received from Host");
        }
    }
}