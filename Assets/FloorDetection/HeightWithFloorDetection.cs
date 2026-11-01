using UnityEngine;

public class HeightWithFloorDetection : MonoBehaviour
{
    private float targetHeight;

    private void OnEnable()
    {
        FloorHeightScan.OnFloorEstimation += MoveToHeight;
    }

    private void OnDisable()
    {
        FloorHeightScan.OnFloorEstimation -= MoveToHeight;
    }

    private void MoveToHeight(float height)
    {
        var pos = transform.position;
        pos.y = targetHeight;
        transform.position = pos;
    }
}
