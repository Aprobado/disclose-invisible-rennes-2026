using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Aprobado.Networking
{
// This version auto connects to the first host it detects if app name is correct.
// Original version in Buzanglo was keeping a server list.
    public class NetworkDiscoveryListener : MonoBehaviour
    {
        [Serializable]
        public struct ServerInfo
        {
            public string Address;
            public string Data;
            public float LastSeen;
        }

        public string networkAppName = "app name"; // used to detect a broadcast for the right app
        public int listenPort = 7778; // Doit matcher le port du broadcaster

        private UdpClient udpClient;
        private bool isListening = false;
        private bool hostFound = false;
        private ServerInfo hostInfo; // we only need address, but let's keep everything for now

        // Utilisés dans le "receiving" thread et "traité" dans main thread (Update)
        private float currentTime; // Time.time est utilisable seulement dans main thread
        // parce que les listeners sont susceptibles d'appeler des methodes uniquement executables dans Main Thread
        // par ex. SetActive(bool)

        private void OnDisable()
        {
            Debug.Log($"Stop network discovery OnDisable");
            StopListening();
        }

        public void StartListening()
        {
            if (isListening) return;

            try
            {
                udpClient = new UdpClient(listenPort);
                // Important pour ne pas bloquer le thread principal
                // On utilise une coroutine ou un thread, ici on fait simple dans Update avec async (si C# 7+) 
                // ou on utilise BeginReceive. Pour Unity simple, BeginReceive est robuste.
                udpClient.BeginReceive(OnReceive, null);
                isListening = true;
                Debug.Log("[Discovery] Écoute sur le port " + listenPort);
            }
            catch (Exception e)
            {
                Debug.LogError("[Discovery] Erreur écoute : " + e.Message);
            }
        }

        private void StopListening()
        {
            if (udpClient != null)
            {
                udpClient.Close();
                udpClient = null;
            }

            isListening = false;
        }

        private void OnReceive(IAsyncResult result)
        {
            Debug.Log($"Received network message in NetworkDiscoveryListener");
            if (!isListening || udpClient == null) return;

            try
            {
                IPEndPoint senderEP = new IPEndPoint(IPAddress.Any, listenPort);
                byte[] data = udpClient.EndReceive(result, ref senderEP);
                string message = Encoding.UTF8.GetString(data);
                string address = senderEP.Address.ToString();

                // if this is the one we're waiting for, save info and flag "found" so that main thread can pick it up.
                if (message == networkAppName)
                {
                    hostInfo = new ServerInfo
                    {
                        Address = address,
                        Data = message,
                        LastSeen = currentTime
                    };
                    hostFound = true;
                }
                else
                {
                    // Relancer l'écoute
                    udpClient.BeginReceive(OnReceive, null);
                }
            }
            catch (Exception e)
            {
                // Ignore les erreurs de fermeture
                if (isListening) Debug.LogError("[Discovery] Erreur réception : " + e.Message);
            }
        }

        public bool JoinGame()
        {
            if (NetworkManager.Singleton == null) return false;
            var transport = NetworkManager.Singleton.NetworkConfig.NetworkTransport as UnityTransport;
            if (transport != null)
            {
                var gamePort = transport.ConnectionData.Port;
                transport.SetConnectionData(hostInfo.Address, gamePort, "0.0.0.0");
                Debug.Log($"Starting Client on {hostInfo.Address}:{gamePort}");
                return NetworkManager.Singleton.StartClient();
            }

            return false;
        }

        private void Update()
        {
            currentTime = Time.time;
            if (!isListening) return;

            if (hostFound)
            {
                hostFound = false;
                if (JoinGame())
                {
                    Destroy(this); // on Join success, destroy the discovery listener
                }
                else
                {
                    // Relancer l'écoute
                    udpClient.BeginReceive(OnReceive, null);
                }
            }
        }
    }
}