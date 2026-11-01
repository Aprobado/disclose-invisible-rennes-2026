using UnityEngine;

namespace Aprobado.XR.Anchors
{
    public class SpatialAnchorIsUnique : MonoBehaviour
    {
        public static bool Instantiated;

        void Awake()
        {
            Instantiated = true;
        }

        private void OnDestroy()
        {
            Instantiated = false;
        }
    }
}