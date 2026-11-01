using System;
using System.Collections.Generic;
using Meta.XR.BuildingBlocks;
using UnityEngine;

namespace Aprobado.XR.Anchors
{
    // Base is copied from SpatialAnchorLocalStorageManagerBuildingBlock
    public class SpatialAnchorLocalStorageManager : MonoBehaviour
    {
        private SpatialAnchorCoreBuildingBlock _spatialAnchorCore;

        private const string NumUuidsPlayerPref = "numUuids";

        private void Start()
        {
            _spatialAnchorCore = FindAnyObjectByType<SpatialAnchorCoreBuildingBlock>();
            _spatialAnchorCore.OnAnchorCreateCompleted.AddListener(SaveAnchorUuidToLocalStorage);
            _spatialAnchorCore.OnAnchorEraseCompleted.AddListener(RemoveAnchorFromLocalStorage);
            _spatialAnchorCore.OnAnchorsEraseAllCompleted.AddListener(RemoveAllAnchorsFromLocalStorage);
        }

        private void SaveAnchorUuidToLocalStorage(OVRSpatialAnchor anchor, OVRSpatialAnchor.OperationResult result)
        {
            if (result != OVRSpatialAnchor.OperationResult.Success)
            {
                return;
            }

            if (!PlayerPrefs.HasKey(NumUuidsPlayerPref))
            {
                PlayerPrefs.SetInt(NumUuidsPlayerPref, 0);
            }

            int playerNumUuids = PlayerPrefs.GetInt(NumUuidsPlayerPref);
            PlayerPrefs.SetString("uuid" + playerNumUuids, anchor.Uuid.ToString());
            PlayerPrefs.SetInt(NumUuidsPlayerPref, ++playerNumUuids);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Remove a <see cref="Guid"/> of specified <see cref="OVRSpatialAnchor"/> from local storage.
        /// </summary>
        private void RemoveAnchorFromLocalStorage(OVRSpatialAnchor anchor, OVRSpatialAnchor.OperationResult result)
        {
            var uuid = anchor.Uuid;
            if (result == OVRSpatialAnchor.OperationResult.Failure)
                return;

            var playerUuidCount = PlayerPrefs.GetInt(NumUuidsPlayerPref, 0);
            for (int i = 0; i < playerUuidCount; i++)
            {
                var key = "uuid" + i;
                var value = PlayerPrefs.GetString(key, "");
                if (value.Equals(uuid.ToString()))
                {
                    var lastKey = "uuid" + (playerUuidCount - 1);
                    var lastValue = PlayerPrefs.GetString(lastKey);
                    PlayerPrefs.SetString(key, lastValue);
                    PlayerPrefs.DeleteKey(lastKey);

                    playerUuidCount--;
                    if (playerUuidCount < 0) playerUuidCount = 0;
                    PlayerPrefs.SetInt(NumUuidsPlayerPref, playerUuidCount);
                    break;
                }
            }
            PlayerPrefs.Save();
        }

        private void RemoveAllAnchorsFromLocalStorage(OVRSpatialAnchor.OperationResult result)
        {
            if (result == OVRSpatialAnchor.OperationResult.Failure)
                return;
            
            var playerUuidCount = PlayerPrefs.GetInt(NumUuidsPlayerPref, 0);
            for (int i = 0; i < playerUuidCount; i++)
            {
                var key = "uuid" + i;
                PlayerPrefs.DeleteKey(key);
            }
            PlayerPrefs.SetInt(NumUuidsPlayerPref, 0);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// This method will load the <see cref="Guid"/>(s) of <see cref="OVRSpatialAnchor"/> from local storage for each one found.
        /// </summary>
        /// <returns>
        /// List of loaded <see cref="Guid"/>(s) from local storage.
        /// </returns>
        public void GetAnchorAnchorUuidFromLocalStorage(List<Guid> uuids)
        {
            // Get number of saved anchor uuids
            if (!PlayerPrefs.HasKey(NumUuidsPlayerPref))
            {
                Reset();
                Debug.Log($"[{nameof(SpatialAnchorLocalStorageManagerBuildingBlock)}] Anchor not found.");
                return;
            }

            // Load unbounded anchors' uuid from local storage
            uuids.Clear();
            var playerUuidCount = PlayerPrefs.GetInt(NumUuidsPlayerPref);
            for (int i = 0; i < playerUuidCount; ++i)
            {
                var uuidKey = "uuid" + i;
                if (!PlayerPrefs.HasKey(uuidKey))
                    continue;

                var currentUuid = PlayerPrefs.GetString(uuidKey);
                uuids.Add(new Guid(currentUuid));
            }
        }

        /// <summary>
        /// Clear all <see cref="OVRSpatialAnchor"/> data from local storage.
        /// </summary>
        /// <remarks>
        /// Sets Unity PlayerPrefs to discard all <see cref="OVRSpatialAnchor"/> data from local storage.
        /// </remarks>
        public void Reset()
        {
            PlayerPrefs.SetInt(NumUuidsPlayerPref, 0);
        }

        private void OnDestroy()
        {
            _spatialAnchorCore.OnAnchorCreateCompleted.RemoveAllListeners();
        }
    }
}
