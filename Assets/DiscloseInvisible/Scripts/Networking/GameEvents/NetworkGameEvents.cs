using System;
using Unity.Netcode;
using UnityEngine;

namespace Aprobado.Networking
{
    public class NetworkGameEvents : NetworkBehaviour
    {
        public static event Action OnDebugNetworkingReceived;

        [Rpc(SendTo.NotServer)]
        public void DebugNetworkingRpc()
        {
            OnDebugNetworkingReceived?.Invoke(); // <= relay le message via un event
        }

        // To debug existing connection, call DebugNetworkingClientRpc every second
        private float delay = 3f;

        private void Update()
        {
            if (NetworkManager.Singleton == null) return;
            if (!NetworkManager.Singleton.IsHost) return;
            delay -= Time.deltaTime;
            if (delay < 0)
            {
                DebugNetworkingRpc();
                delay = 3f;
            }
        }

        // Exemple d'envoi à un seul user:
        // On utilise RpcParams pour cibler un seul client
        // public void DistributeEnonceToTargetedPlayer(int enonceId, float timeRemaining, ulong clientId)
        // {
        //     DistributeEnonceToPlayerClientRpc(enonceId, timeRemaining, CreateTargetClientRpcParams(clientId));
        // }
        //
        // // Remplace SendSelectedEnonceToPlayer (INDIVIDUEL)
        // [ClientRpc]
        // private void DistributeEnonceToPlayerClientRpc(int enonceId, float timeRemaining, ClientRpcParams rpcParams = default)
        // {
        //     ClientMessageReceiver.EnonceReceived(enonceId, timeRemaining);
        // }
        //
        // [ClientRpc]
        // private void AskPlayerClientRpc(ClientRpcParams rpcParams = default)
        // {
        //     ClientMessageReceiver.AskPlayer();
        // }
    }
}