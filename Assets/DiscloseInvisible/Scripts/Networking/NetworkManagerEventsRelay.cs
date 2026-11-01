using System;
using Unity.Netcode;
using UnityEngine;

namespace Aprobado.Networking
{
// Utility script that handles subscribing to NetworkManager and converts
// Singleton events to static events to not bother with verbose tests when subbing to network events.
    public class NetworkManagerEventsRelay : MonoBehaviour
    {
        public static event Action OnServerStarted;
        public static event Action<bool> OnServerStopped;
        public static event Action OnClientStarted;
        public static event Action<bool> OnClientStopped;
        public static event Action<ulong> OnClientConnectedCallback;
        public static event Action<ulong> OnClientDisconnectCallback;
        public static event Action<NetworkManager, ConnectionEventData> OnConnectionEvent;

        private void OnEnable()
        {
            TrySubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromServerEvents();
        }

        private void TrySubscribeToEvents()
        {
            if (NetworkManager.Singleton != null)
            {
                SubscribeToServerEvents();
                return;
            }

            Invoke(nameof(TrySubscribeToEvents), 0.5f);
        }

        private void SubscribeToServerEvents()
        {
            UnsubscribeFromServerEvents();
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnServerStarted += OnServerStartedListener;
                NetworkManager.Singleton.OnServerStopped += OnServerStoppedListener;
                NetworkManager.Singleton.OnClientStarted += OnClientStartedListener;
                NetworkManager.Singleton.OnClientStopped += OnClientStoppedListener;
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallbackListener;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallbackListener;
                NetworkManager.Singleton.OnConnectionEvent += OnConnectionEventListener;
            }
        }

        private void UnsubscribeFromServerEvents()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnServerStarted -= OnServerStartedListener;
                NetworkManager.Singleton.OnServerStopped -= OnServerStoppedListener;
                NetworkManager.Singleton.OnClientStarted -= OnClientStartedListener;
                NetworkManager.Singleton.OnClientStopped -= OnClientStoppedListener;
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallbackListener;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallbackListener;
                NetworkManager.Singleton.OnConnectionEvent -= OnConnectionEventListener;
            }
        }

        private void OnServerStartedListener()
        {
            OnServerStarted?.Invoke();
        }

        private void OnServerStoppedListener(bool isHost)
        {
            OnServerStopped?.Invoke(isHost);
        }

        private void OnClientStartedListener()
        {
            OnClientStarted?.Invoke();
        }

        private void OnClientStoppedListener(bool isHost)
        {
            OnClientStopped?.Invoke(isHost);
        }

        private void OnClientConnectedCallbackListener(ulong connectionId)
        {
            OnClientConnectedCallback?.Invoke(connectionId);
        }

        private void OnClientDisconnectCallbackListener(ulong connectionId)
        {
            OnClientDisconnectCallback?.Invoke(connectionId);
        }

        private void OnConnectionEventListener(NetworkManager networkManager, ConnectionEventData eventData)
        {
            OnConnectionEvent?.Invoke(networkManager, eventData);
        }
    }
}