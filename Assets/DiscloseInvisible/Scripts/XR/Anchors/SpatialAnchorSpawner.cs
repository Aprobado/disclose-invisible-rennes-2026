using Meta.XR.BuildingBlocks;
using UnityEngine;

namespace Aprobado.XR.Anchors
{
    // Base is copied from SpatialAnchorSpawnerBuildingBlock
    public class SpatialAnchorSpawner : MonoBehaviour
    {
        /// <summary>
        /// A prefab to instantiate.
        /// </summary>
        public GameObject AnchorPreview
        {
            get => _anchorPreview;
            set
            {
                _anchorPreview = value;
                if (_anchorPreviewTransform) Destroy(_anchorPreviewTransform.gameObject);
                _anchorPreviewTransform = Instantiate(AnchorPreview).transform;
            }
        }

        [Tooltip("A placeholder object to place in the anchor's position.")]
        [SerializeField]
        private GameObject _anchorPreview;
        
        [Tooltip("A the prefab placed as anchor.")]
        [SerializeField]
        private GameObject _anchorPrefab;

        public GameObject AnchorPrefab => _anchorPrefab;

        private SpatialAnchorCoreBuildingBlock _spatialAnchorCore;
        private OVRCameraRig _cameraRig;
        private Transform _anchorPreviewTransform;

        private void Awake()
        {
            _spatialAnchorCore = FindAnyObjectByType<SpatialAnchorCoreBuildingBlock>();
            // _spatialAnchorCore.OnAnchorsEraseAllCompleted.AddListener(OnAnchorsEraseAllCompleted);
            _cameraRig = FindAnyObjectByType<OVRCameraRig>();
            AnchorPreview = _anchorPreview;
            AttachPreviewToHand();
        }

        // private void OnAnchorsEraseAllCompleted(OVRSpatialAnchor.OperationResult result)
        // {
        //     AttachPreviewToHand();
        // }
        //
        private void AttachPreviewToHand()
        {
            _anchorPreviewTransform.parent = _cameraRig.rightControllerAnchor;
            _anchorPreviewTransform.localPosition = Vector3.zero;
            _anchorPreviewTransform.localRotation = Quaternion.identity;
        }
        //
        // private void DetachPreviewFromHand()
        // {
        //     _anchorPreviewTransform.parent = null;
        // }

        /// <summary>
        /// Spawns a new <see cref="OVRSpatialAnchor"/> for the <see cref="AnchorPreview"/> in the provided position and the provided orientation.
        /// </summary>
        /// <param name="position">Position for the new anchor.</param>
        /// <param name="rotation">Orientation of the new anchor</param>
        public void SpawnSpatialAnchor(Vector3 position, Quaternion rotation)
        {
            if (SpatialAnchorIsUnique.Instantiated) return;
            if (!AnchorEditMode.IsActive) return;
            _spatialAnchorCore.InstantiateSpatialAnchor(_anchorPrefab, position, rotation);
            // DetachPreviewFromHand();
        }

        public void SpawnSpatialAnchor()
        {
            var position = _anchorPreviewTransform.position;
            var rotation = _anchorPreviewTransform.rotation;
            SpawnSpatialAnchor(position, rotation);
        }

        public void EraseAllAnchors()
        {
            if (!SpatialAnchorIsUnique.Instantiated) return;
            if (!AnchorEditMode.IsActive) return;
            _spatialAnchorCore.EraseAllAnchors();
        }
    }
}
