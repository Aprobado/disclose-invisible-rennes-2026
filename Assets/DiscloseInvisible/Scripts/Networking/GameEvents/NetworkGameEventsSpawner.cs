using Unity.Netcode;
using UnityEngine;

namespace Aprobado.Networking
{
    public class NetworkGameEventsSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject relayPrefab;
        private NetworkObject relay;

        private void OnEnable()
        {
            NetworkManagerEventsRelay.OnServerStarted += OnServerStarted;
            NetworkManagerEventsRelay.OnServerStopped += OnServerStopped;
            // NetworkManagerEventsRelay.OnClientConnectedCallback += OnClientConnectedCallback;
        }

        private void OnDisable()
        {
            NetworkManagerEventsRelay.OnServerStarted -= OnServerStarted;
            NetworkManagerEventsRelay.OnServerStopped -= OnServerStopped;
            // NetworkManagerEventsRelay.OnClientConnectedCallback -= OnClientConnectedCallback;
        }

        private void OnServerStarted()
        {
            Debug.Log($"Server started, spawn Relay");
            var instance = Instantiate(relayPrefab);
            relay = instance.GetComponent<NetworkObject>();
            relay.Spawn();
        }

        private void OnServerStopped(bool isHost)
        {
            Debug.Log($"Server stopped, discard Relay reference (the object is destroyed by Network Shutdown?)");
            relay = null;
        }

        // Is received on Server/Host when a client connects
        // private void OnClientConnectedCallback(ulong connectionId)
        // {
        //     Debug.Log($"Client connected, Relay is spawned on non server");
        //     if (!NetworkManager.Singleton.IsServer) return;
        //     if (connectionId == NetworkManager.Singleton.LocalClientId) return; // if host
        //     if (relay == null) return;
        //     
        //     Debug.Log($"OnClientConnectedCallback, id {connectionId}");
        //     
        //     relay.NetworkShow(connectionId);
        // }
    }
}