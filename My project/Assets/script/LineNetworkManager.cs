using System.Collections.Generic;
using UnityEngine;

public class LineNetworkManager : MonoBehaviour
{
    public Material lineMaterial;
    public float lineWidth = 0.1f;

    private List<LineRenderer> lines = new List<LineRenderer>();

    void Start()
    {
        GenerateLines();
    }

    void GenerateLines()
    {
        Waypoint[] waypoints = FindObjectsOfType<Waypoint>();

        foreach (var wp in waypoints)
        {
            foreach (var target in wp.connectedWaypoints)
            {
                // 为每一条连接创建一个 LineRenderer
                CreateLine(wp.transform.position, target.transform.position);
            }
        }
    }

    void CreateLine(Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("Line");
        lineObj.transform.parent = transform;

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();

        lr.material = lineMaterial;
        lr.positionCount = 2;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;

        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        // 关闭 Z 轴影响，保证线在 2D 中显示
        lr.sortingOrder = -1;

        lines.Add(lr);
    }
}
