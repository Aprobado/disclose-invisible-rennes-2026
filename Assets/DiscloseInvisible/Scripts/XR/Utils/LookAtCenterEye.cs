using UnityEngine;

public class LookAtCenterEye : MonoBehaviour
{
    private OVRCameraRig cameraRig;
    private Transform target;
    
    void Awake()
    {
        cameraRig = FindAnyObjectByType<OVRCameraRig>();
        if (cameraRig != null) target = cameraRig.centerEyeAnchor;
        if (target == null && Camera.main != null) target = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null)
        {
            enabled = false;
            return;
        }
        transform.rotation = Quaternion.LookRotation(transform.position - target.position);
    }
}
