// Adapted from SpatialAnchorLoaderBuildingBlock

using System;
using System.Collections.Generic;
using Meta.XR.BuildingBlocks;
using UnityEngine;

namespace Aprobado.XR.Anchors
{
    /// <summary>
    /// This is a utility class that is responsible for storing and loading an <see cref="OVRSpatialAnchor"/>
    /// from local storage.
    /// </summary>
    [RequireComponent(typeof(SpatialAnchorSpawner))]
    public class SpatialAnchorLoader : MonoBehaviour
    {
        private SpatialAnchorCoreBuildingBlock _spatialAnchorCore;
        private SpatialAnchorSpawner _spatialAnchorSpawner;
        private List<Guid> _uuids;

        private void Awake()
        {
            _spatialAnchorSpawner = GetComponent<SpatialAnchorSpawner>();
            _spatialAnchorCore = FindAnyObjectByType<SpatialAnchorCoreBuildingBlock>();
        }

        private void Start()
        {
            _uuids = new List<Guid>(10);
        }

        /// <summary>
        /// Loads and instantiates a set of <see cref="OVRSpatialAnchor"/>s from a list of uuids.
        /// </summary>
        public virtual void LoadAndInstantiateAnchors(List<Guid> uuids)
        {
            _spatialAnchorCore.LoadAndInstantiateAnchors(_spatialAnchorSpawner.AnchorPrefab, uuids);
        }

        /// <summary>
        /// Loads an <see cref="OVRSpatialAnchor"/> from local storage.
        /// </summary>
        /// <remarks>A <see cref="SpatialAnchorLocalStorageManagerBuildingBlock"/> component is required to load anchors from
        /// default local storage.</remarks>
        /// <remarks>If </remarks>
        public virtual void LoadAnchorsFromDefaultLocalStorage()
        {
            var spatialAnchorLocalStorageManager = FindAnyObjectByType<SpatialAnchorLocalStorageManager>();
            if (!spatialAnchorLocalStorageManager)
            {
                Debug.Log($"[{nameof(SpatialAnchorLocalStorageManager)}] component is missing.");
                return;
            }

            spatialAnchorLocalStorageManager.GetAnchorAnchorUuidFromLocalStorage(_uuids);
            if (_uuids.Count > 0)
            {
                _spatialAnchorCore.LoadAndInstantiateAnchors(_spatialAnchorSpawner.AnchorPrefab, _uuids);
            }
        }
    }
}
