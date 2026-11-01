using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace Aprobado.Networking
{
    public class NetworkBroadcaster : MonoBehaviour
    {
        [Header("Settings")] public int broadcastPort = 7778; // Doit être différent que le port de jeu
        public string broadcastMessage = "-"; // On envoie le numéro de table. Il correspond à la fin de l'adresse IP
        public float broadcastInterval = 1.0f;

        private UdpClient udpClient;
        private IPEndPoint broadcastEndPoint;
        private float timer;
        private bool isBroadcasting = false;

        private void OnDisable()
        {
            StopBroadcasting();
        }

        public void StartBroadcasting()
        {
            if (isBroadcasting) return;

            try
            {
                udpClient = new UdpClient();
                udpClient.EnableBroadcast = true;
                // 255.255.255.255 diffuse sur tout le réseau local
                broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, broadcastPort);
                isBroadcasting = true;
                Debug.Log("[Broadcaster] Démarré sur le port " + broadcastPort);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[Broadcaster] Erreur : " + e.Message);
            }
        }

        private void StopBroadcasting()
        {
            Debug.Log($"[Broadcaster] Stopping");
            if (udpClient != null)
            {
                udpClient.Close();
                udpClient = null;
            }

            isBroadcasting = false;
        }

        private void Update()
        {
            if (!isBroadcasting || udpClient == null) return;

            timer += Time.deltaTime;
            if (timer >= broadcastInterval)
            {
                timer = 0;
                SendBroadcast();
            }
        }

        private void SendBroadcast()
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(broadcastMessage);
                udpClient.Send(data, data.Length, broadcastEndPoint);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[Broadcaster] Erreur d'envoi : " + e.Message);
            }
        }
    }
}