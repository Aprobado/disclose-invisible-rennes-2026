using UnityEngine;

namespace Aprobado.XR.Anchors
{
    public class RenderOnlyInEditMode : MonoBehaviour
    {
        private Renderer[] renderers;
        
        void Start()
        {
            renderers = GetComponentsInChildren<Renderer>(true);
        }

        void Update()
        {
            foreach (var rend in renderers)
            {
                rend.enabled = AnchorEditMode.IsActive;
            }
        }
    }
}