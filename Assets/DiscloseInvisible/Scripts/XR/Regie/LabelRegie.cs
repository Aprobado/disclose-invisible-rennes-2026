using TMPro;
using UnityEngine;

namespace Aprobado.XR.Regie
{
    // Place this on a TextMeshPro object parented above the controller.
    [RequireComponent(typeof(TextMeshPro))]
    public class LabelRegie : MonoBehaviour
    {
        [SerializeField] private OVRCameraRig cameraRig;
        [SerializeField] private ModeRegie modeRegie;
        [SerializeField] private bool faceCamera = true;

        private TextMeshPro tmp;
        private Transform cam;

        private void Awake()
        {
            if (cameraRig == null) cameraRig = FindAnyObjectByType<OVRCameraRig>();
            if (cameraRig != null)
            {
                cam = cameraRig.centerEyeAnchor;
                transform.parent = cameraRig.rightControllerAnchor;
                transform.localPosition = new Vector3(0f, 0.06f, 0.04f);
            }
            tmp = GetComponent<TextMeshPro>();
            tmp.enabled = false;
        }

        // LateUpdate so ModeRegie has already updated IsActive and the current régie this frame
        private void LateUpdate()
        {
            var visible = ModeRegie.IsActive && modeRegie != null;
            tmp.enabled = visible;
            if (!visible) return;

            tmp.text = modeRegie.GetCurrentRegieLabel();
            
            if (!faceCamera) return;
            if (cam == null && Camera.main != null) cam = Camera.main.transform;
            if (cam != null) transform.rotation = Quaternion.LookRotation(transform.position - cam.position);
        }
    }
}
