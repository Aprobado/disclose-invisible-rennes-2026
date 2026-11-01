using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Aprobado.Networking
{
    // This manager is either always Host or always Client.
    // If Host, add a broadcast emitter for as long as the object exists.
    public class NetworkAutoConnectManager : MonoBehaviour
    {
        public enum NetworkRole
        {
            HOST,
            CLIENT
        }

        [SerializeField] private NetworkRole networkRole;
        public NetworkRole Role => networkRole;
        [SerializeField] private int broadcastPort = 7778; // Doit être différent que le port de jeu
        [SerializeField] private string networkAppName;
        private NetworkDiscoveryListener discoveryListener;

        private bool started = false;
        

        IEnumerator Start()
        {
            var manager = NetworkManager.Singleton;
            
            Debug.Log($"NetworkManager exists : {manager != null}");
            
            yield return new WaitForSeconds(2f); // be sure the scene is loaded and ready before starting Network
            
            var isHost = networkRole == NetworkRole.HOST || Application.isEditor;
            
            if (isHost)
            {
                var transport = NetworkManager.Singleton.NetworkConfig.NetworkTransport as UnityTransport;
                if (transport != null)
                {
                    transport.SetConnectionData("127.0.0.1", transport.ConnectionData.Port, "0.0.0.0");
                }
                StartHost();
                var broadcaster = gameObject.AddComponent<NetworkBroadcaster>();
                broadcaster.broadcastPort = broadcastPort;
                broadcaster.broadcastMessage = networkAppName;
                broadcaster.StartBroadcasting();
            }
            else
            {
                StartDiscovery();
            }
            
            started = true;
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton != null) NetworkManager.Singleton.Shutdown();
        }

        // If connection is lost, we can try to reconnect by adding discovery again. Needs testing.
        private void Update()
        {
            if (!started || networkRole != NetworkRole.CLIENT || Application.isEditor) return;
            
            var manager = NetworkManager.Singleton;
            // manager must exist, not be connected and not shutting down
            if (manager == null || manager.IsClient || manager.ShutdownInProgress) return;
            if (discoveryListener == null) StartDiscovery();
        }

        public void StartHost()
        {
            NetworkManager.Singleton.StartHost();
        }
        
        private void StartDiscovery()
        {
            discoveryListener = gameObject.AddComponent<NetworkDiscoveryListener>();
            discoveryListener.listenPort = broadcastPort;
            discoveryListener.networkAppName = networkAppName;
            discoveryListener.StartListening();
        }
    }
}