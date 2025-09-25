using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class GesturePathVisualizer : MonoBehaviour
{
    [Header("Settings")]
    public float minDistance = 0.1f;
    public float pointLifetime = 1.5f;

    private LineRenderer lineRenderer;
    private List<Vector3> points = new List<Vector3>();
    private List<float> pointTimes = new List<float>();
    private bool drawing = false;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }


    void Update()
    {
        UpdatePointLifetime();

        var touch = Touchscreen.current?.primaryTouch;
        if (touch == null) return;

        bool isPressed = touch.press.isPressed;
        Vector2 screenPos = touch.position.ReadValue();

        if (isPressed)
        {
            Vector3 worldPos = GetWorldPosition(screenPos);
            Vector3 localPos = transform.InverseTransformPoint(worldPos);

            if (!drawing)
            {
                drawing = true;
                AddPoint(localPos);
            }
            else
            {
                AddPoint(localPos);
            }
        }
        else if (drawing)
        {
            drawing = false;
        }
    }

    Vector3 GetWorldPosition(Vector2 screenPos)
    {
        Camera cam = Camera.main;
        float distance = 3f;

        Ray ray = cam.ScreenPointToRay(screenPos);
        Vector3 planeCenter = cam.transform.position + cam.transform.forward * distance;
        Plane plane = new Plane(-cam.transform.forward, planeCenter);

        if (plane.Raycast(ray, out float rayDistance))
            return ray.GetPoint(rayDistance);

        return cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, distance));
    }

    void UpdatePointLifetime()
    {
        float currentTime = Time.time;
        bool pointsRemoved = false;

        for (int i = pointTimes.Count - 1; i >= 0; i--)
        {
            if (currentTime - pointTimes[i] > pointLifetime)
            {
                points.RemoveAt(i);
                pointTimes.RemoveAt(i);
                pointsRemoved = true;
            }
        }

        if (pointsRemoved)
            UpdateLineRenderer();
    }

    void UpdateLineRenderer()
    {
        lineRenderer.positionCount = points.Count;
        for (int i = 0; i < points.Count; i++)
            lineRenderer.SetPosition(i, points[i]);
    }

    void AddPoint(Vector3 point)
    {
        float distance = 0f;
        if (points.Count > 0)
            distance = Vector3.Distance(points[points.Count - 1], point);

        if (points.Count == 0 || distance > minDistance)
        {
            points.Add(point);
            pointTimes.Add(Time.time);

            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPosition(points.Count - 1, point);
        }
    }
}