using System;
using System.Collections.Generic;
using Aprobado.XR.Anchors;
using Meta.XR;
using UnityEngine;
using UnityEngine.Events;

public class FloorHeightScan : MonoBehaviour
{
    [SerializeField] private Transform centerEye;
    [SerializeField] private Transform debugfloor;
    [SerializeField] private GameObject debugHit;

    [Header("Ray grid")]
    [Tooltip("Number of cells along the flat head direction")]
    [SerializeField, Min(1)] private int rows = 6;
    [Tooltip("Number of cells across the flat head direction")]
    [SerializeField, Min(1)] private int columns = 6;
    [Tooltip("Size of the targeted floor grid in meters (x = width, y = depth)")]
    [SerializeField] private Vector2 gridSize = new Vector2(2f, 2f);
    [Tooltip("Distance in meters from below the headset to the grid center, along the flat head direction")]
    [SerializeField] private float gridForwardOffset = 1f;
    [Tooltip("Expected headset height above the floor, used to aim the rays")]
    [SerializeField] private float assumedHeadHeight = 1.7f;
    [Tooltip("Hits needed at the same height to validate the floor")]
    [SerializeField, Min(1)] private int minHitsForFloor = 12;
    private bool activateDebug;
    
    public static event Action<float> OnFloorEstimation;
    public UnityEvent<float> OnFloorEstimationEvent;
    
    private EnvironmentRaycastManager raycastManager;
    private List<Transform> rays;

    private List<GameObject> hitDebugs;

    private float currentHeight;
    private float targetHeight;
    private float currentVelocity;

    private void Awake()
    {
        raycastManager = FindFirstObjectByType<EnvironmentRaycastManager>();
    }

    private void Start()
    {
        rays = new List<Transform>();
        hitDebugs = new List<GameObject>();
        CreateRayGrid();
    }

    private void CreateRayGrid()
    {
        var grid = new GameObject("RayGrid").transform;
        grid.SetParent(transform, false);
        var debugContainer = new GameObject("HitDebugs").transform;
        debugContainer.SetParent(transform, false);

        for (var r = 0; r < rows; r++)
        {
            var z = gridForwardOffset + (-0.5f + (r + 0.5f) / rows) * gridSize.y;
            for (var c = 0; c < columns; c++)
            {
                var x = (-0.5f + (c + 0.5f) / columns) * gridSize.x;
                var ray = new GameObject($"Ray_{r}_{c}").transform;
                ray.SetParent(grid, false);
                // aim at the cell center on the expected floor, relative to the flat forward set in Update
                var target = new Vector3(x, -assumedHeadHeight, z);
                ray.localRotation = Quaternion.LookRotation(target, Vector3.forward);
                rays.Add(ray);
                hitDebugs.Add(Instantiate(debugHit, debugContainer));
            }
        }
    }

    void Update()
    {
        activateDebug = AnchorEditMode.IsActive;// OVRInput.Get(OVRInput.RawButton.LHandTrigger);
        
        transform.position = centerEye.position;
        var origin = transform.position;
        var forward = centerEye.forward;
        forward.y = 0;
        forward.Normalize();
        transform.forward = forward;
        
        var hits = new List<Vector3>();
        Vector3 hit;
        var plane = new Plane(Vector3.up, Vector3.zero);
        for(var i = 0; i < rays.Count; i++)
        {
            var ray = rays[i];
            #if !UNITY_EDITOR
            if (EnvironmentAim(ray.forward, out hit))
            {
                hits.Add(hit);
                hitDebugs[i].transform.position = hit;
                hitDebugs[i].SetActive(activateDebug);
            }
            else
            {
                hitDebugs[i].SetActive(false);
            }
            #else
            var actualRay = new Ray(origin, ray.forward);
            if (plane.Raycast(actualRay, out var enter))
            {
                hit = actualRay.GetPoint(enter);
                hitDebugs[i].transform.position = hit;
                hitDebugs[i].SetActive(activateDebug);
            }
            else
            {
                hitDebugs[i].SetActive(false);
            }
            #endif
        }
        
        if (EvaluateFloor(hits, out var estimatedHeight))
        {
            targetHeight = estimatedHeight;// + 0.02f; // adding small bias to avoid element being too close to the floor
        }

        currentHeight = Mathf.SmoothDamp(currentHeight, targetHeight, ref currentVelocity, 1f, 1f);
        OnFloorEstimation?.Invoke(currentHeight);
        OnFloorEstimationEvent?.Invoke(currentHeight);
        if (debugfloor != null)
        {
            debugfloor.position = new Vector3(transform.position.x, currentHeight, transform.position.z);
            debugfloor.gameObject.SetActive(activateDebug);
        }
    }
    
    private bool EnvironmentAim(Vector3 direction, out Vector3 hitPosition)
    {
        var ray = new Ray(transform.position, direction);
        hitPosition = new Vector3();
        if (Vector3.Angle(ray.direction, Vector3.down) > 80) return false;
        
        if (raycastManager.Raycast(ray, out var hit, 10f))
        {
            if (hit.status == EnvironmentRaycastHitStatus.Hit)
            {
                if (Vector3.Distance(hit.point, transform.position) < 0.1f) return false;    // min distance : 50cm
                hitPosition = hit.point;
                // hitNormal = hit.normalConfidence > 0 ? hit.normal : Vector3.up;
                return true;
            }
            else
            {
                if (hit.status is EnvironmentRaycastHitStatus.NotReady or EnvironmentRaycastHitStatus.NotSupported)
                {
                    Debug.Log($"Problem with raycaster. Not Ready or Not Supported");
                }
            }
        }
        
        return false;
    }
    
    private bool EvaluateFloor(List<Vector3> hits, out float estimatedHeight)
    {
        estimatedHeight = 0f;

        if (hits == null || hits.Count < 4)
            return false;

        const float heightTolerance = 0.05f; // tweak based on your scale

        // Extract Y values
        List<float> heights = new List<float>(hits.Count);
        foreach (var hit in hits)
            heights.Add(hit.y);

        var smallest = Mathf.Infinity;
        // Try each height as a candidate "floor"
        for (int i = 0; i < heights.Count; i++)
        {
            float reference = heights[i];
            int count = 0;
            float sum = 0f;
            bool isLowest = true;

            for (int j = 0; j < heights.Count; j++)
            {
                if (Mathf.Abs(heights[j] - reference) <= heightTolerance)
                {
                    count++;
                    sum += heights[j];
                }
                else if (heights[j] < reference)
                {
                    isLowest = false;
                    break;
                }
            }

            if (!isLowest) continue; // if a point is lower than current tested surface, try next

            if (count >= minHitsForFloor)
            {
                estimatedHeight = sum / count; // average for stability
                return true;
            }
        }

        return false;
    }
}
