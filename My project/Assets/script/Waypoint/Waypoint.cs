using System.Collections.Generic;
using UnityEngine;

public class Waypoint2D : MonoBehaviour
{
    [Header("路点设置")]
    public string waypointName = "路点";
    public Color waypointColor = Color.cyan;

    [Header("连接的路点")]
    public List<Waypoint2D> connectedWaypoints = new List<Waypoint2D>();

    [Header("可视化设置")]
    public bool showConnectionsInGame = true; // 在游戏中显示连接线
    public float connectionLineWidth = 0.1f; // 连接线宽度

    [Header("交互设置")]
    public Color hoverColor = Color.yellow; // 鼠标悬停颜色
    public Color clickableColor = Color.green; // 可点击时的颜色
    public Color unavailableColor = Color.gray; // 不可点击时的颜色

    private float gizmoRadius = 0.5f;
    private List<LineRenderer> connectionLines = new List<LineRenderer>();
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isHovered = false;
    private bool isClickable = false;

    void Start()
    {
        // 获取SpriteRenderer组件
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // 添加碰撞体用于鼠标检测
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f;
            collider.isTrigger = true;
        }

        if (showConnectionsInGame)
        {
            CreateConnectionLines();
        }
    }

    // 创建连接线（在游戏运行时显示）
    private void CreateConnectionLines()
    {
        // 清除旧的线
        foreach (var line in connectionLines)
        {
            if (line != null)
                Destroy(line.gameObject);
        }
        connectionLines.Clear();

        // 为每个连接创建线
        foreach (var waypoint in connectedWaypoints)
        {
            if (waypoint != null && waypoint.GetInstanceID() > GetInstanceID())
            {
                GameObject lineObj = new GameObject($"Line_{waypointName}_to_{waypoint.waypointName}");
                lineObj.transform.SetParent(transform);

                LineRenderer line = lineObj.AddComponent<LineRenderer>();
                line.startWidth = connectionLineWidth;
                line.endWidth = connectionLineWidth;
                line.material = new Material(Shader.Find("Sprites/Default"));
                line.startColor = Color.yellow;
                line.endColor = Color.yellow;
                line.sortingOrder = -1; // 在背景层显示

                line.positionCount = 2;
                line.SetPosition(0, transform.position);
                line.SetPosition(1, waypoint.transform.position);

                connectionLines.Add(line);
            }
        }
    }

    void Update()
    {
        // 实时更新线的位置（如果路点移动）
        if (showConnectionsInGame)
        {
            UpdateConnectionLines();
        }

        // 更新颜色显示
        UpdateVisuals();
    }

    // 更新视觉效果
    private void UpdateVisuals()
    {
        if (spriteRenderer == null) return;

        if (isHovered && isClickable)
        {
            spriteRenderer.color = hoverColor;
        }
        else if (isClickable)
        {
            spriteRenderer.color = clickableColor;
        }
        else if (!isClickable)
        {
            spriteRenderer.color = unavailableColor;
        }
        else
        {
            spriteRenderer.color = originalColor;
        }
    }

    // 设置是否可点击
    public void SetClickable(bool clickable)
    {
        isClickable = clickable;
    }

    // 鼠标进入
    void OnMouseEnter()
    {
        isHovered = true;
    }

    // 鼠标离开
    void OnMouseExit()
    {
        isHovered = false;
    }

    // 鼠标点击
    void OnMouseDown()
    {
        if (isClickable)
        {
            // 通知玩家控制器
            PlayerWaypointController2D player = FindObjectOfType<PlayerWaypointController2D>();
            if (player != null)
            {
                player.MoveToWaypoint(this);
            }
        }
        else
        {
            Debug.Log($"{waypointName} 当前不可到达");
        }
    }

    private void UpdateConnectionLines()
    {
        int lineIndex = 0;
        foreach (var waypoint in connectedWaypoints)
        {
            if (waypoint != null && waypoint.GetInstanceID() > GetInstanceID())
            {
                if (lineIndex < connectionLines.Count && connectionLines[lineIndex] != null)
                {
                    connectionLines[lineIndex].SetPosition(0, transform.position);
                    connectionLines[lineIndex].SetPosition(1, waypoint.transform.position);
                }
                lineIndex++;
            }
        }
    }

    // 检查是否与指定路点相连
    public bool IsConnectedTo(Waypoint2D other)
    {
        return connectedWaypoints.Contains(other);
    }

    // 添加连接（双向）
    public void AddConnection(Waypoint2D other)
    {
        if (!connectedWaypoints.Contains(other))
        {
            connectedWaypoints.Add(other);
        }

        if (!other.connectedWaypoints.Contains(this))
        {
            other.connectedWaypoints.Add(this);
        }

        if (showConnectionsInGame && Application.isPlaying)
        {
            CreateConnectionLines();
        }
    }

    // 移除连接（双向）
    public void RemoveConnection(Waypoint2D other)
    {
        connectedWaypoints.Remove(other);
        other.connectedWaypoints.Remove(this);

        if (showConnectionsInGame && Application.isPlaying)
        {
            CreateConnectionLines();
        }
    }

    // 在Scene视图中绘制路点和连接线
    private void OnDrawGizmos()
    {
        // 绘制路点圆形
        Gizmos.color = waypointColor;
        DrawCircle(transform.position, gizmoRadius, 20);

        // 绘制连接线
        if (connectedWaypoints != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var waypoint in connectedWaypoints)
            {
                if (waypoint != null)
                {
                    // 只绘制到ID较大的路点，避免重复
                    if (waypoint.GetInstanceID() > GetInstanceID())
                    {
                        Gizmos.DrawLine(transform.position, waypoint.transform.position);

                        // 绘制箭头指示方向（可选）
                        DrawArrow(transform.position, waypoint.transform.position);
                    }
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 选中时高亮显示
        Gizmos.color = Color.green;
        DrawCircle(transform.position, gizmoRadius * 1.2f, 30);

        // 高亮显示连接的路点
        if (connectedWaypoints != null)
        {
            foreach (var waypoint in connectedWaypoints)
            {
                if (waypoint != null)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(transform.position, waypoint.transform.position);
                    DrawCircle(waypoint.transform.position, gizmoRadius * 0.8f, 20);
                }
            }
        }
    }

    // 绘制圆形
    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0
            );
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }

    // 绘制箭头（表示双向连接）
    private void DrawArrow(Vector3 from, Vector3 to)
    {
        Vector3 direction = (to - from).normalized;
        Vector3 center = (from + to) / 2;

        // 绘制小箭头
        Vector3 right = new Vector3(-direction.y, direction.x, 0) * 0.2f;
        Gizmos.DrawLine(center, center - direction * 0.3f + right);
        Gizmos.DrawLine(center, center - direction * 0.3f - right);
    }
}