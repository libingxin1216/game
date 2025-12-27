using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWaypointController2D : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f;
    public float arrivalDistance = 0.1f;

    [Header("翻转设置")]
    public bool autoFlip = true; // 自动根据移动方向翻转
    public Transform visualTransform; // 视觉模型的Transform（用于翻转）

    [Header("当前状态")]
    public Waypoint2D currentWaypoint;
    public bool isMoving = false;

    private Waypoint2D targetWaypoint;
    private List<Waypoint2D> availableWaypoints = new List<Waypoint2D>();
    private SpriteRenderer spriteRenderer;
    private bool facingRight = true;

    void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // 如果没有指定visualTransform，尝试找到SpriteRenderer所在的对象
        if (visualTransform == null && spriteRenderer != null)
        {
            visualTransform = spriteRenderer.transform;
        }

        // 如果没有设置起始路点，尝试找到最近的路点
        

        // 如果设置了起始路点，移动到该位置
        if (currentWaypoint != null)
        {
            Vector3 pos = currentWaypoint.transform.position;
            pos.z = transform.position.z; // 保持原有的Z坐标
            transform.position = pos;
            UpdateAvailableWaypoints();
        }
    }

    void Update()
    {
        if (isMoving)
        {
            MoveToTarget();
        }
        else
        {
            HandleInput();
        }
    }

    // 处理输入
    private void HandleInput()
    {
        if (availableWaypoints.Count == 0)
            return;

        // 使用数字键选择路点（1-9）
        for (int i = 0; i < Mathf.Min(availableWaypoints.Count, 9); i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectWaypoint(i);
            }
        }
    }

    // 选择路点并开始移动
    public void SelectWaypoint(int index)
    {
        if (index >= 0 && index < availableWaypoints.Count)
        {
            targetWaypoint = availableWaypoints[index];
            isMoving = true;
            Debug.Log($"前往路点: {targetWaypoint.waypointName}");
        }
    }

    // 直接选择指定路点
    public void MoveToWaypoint(Waypoint2D waypoint)
    {
        if (currentWaypoint != null && currentWaypoint.IsConnectedTo(waypoint))
        {
            targetWaypoint = waypoint;
            isMoving = true;
            Debug.Log($"前往路点: {targetWaypoint.waypointName}");
        }
        else
        {
            Debug.LogWarning("该路点不可达！");
        }
    }

    // 移动到目标路点
    private void MoveToTarget()
    {
        if (targetWaypoint == null)
        {
            isMoving = false;
            return;
        }

        Vector3 targetPos = targetWaypoint.transform.position;
        targetPos.z = transform.position.z; // 保持Z坐标不变

        Vector3 direction = targetPos - transform.position;
        float distance = direction.magnitude;

        // 到达目标点
        if (distance <= arrivalDistance)
        {
            transform.position = targetPos;
            currentWaypoint = targetWaypoint;
            targetWaypoint = null;

            // 立即设置为停止状态
            isMoving = false;

            // 立即更新可用路点
            UpdateAvailableWaypoints();

            Debug.Log($"到达路点: {currentWaypoint.waypointName}");
            ShowAvailableWaypoints();

            // 强制刷新动画状态（如果有Animator）
            BroadcastMessage("OnWaypointReached", SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            // 移动
            Vector3 movement = direction.normalized * moveSpeed * Time.deltaTime;
            transform.position += movement;

            // 根据移动方向翻转角色
            if (autoFlip)
            {
                Flip(direction.x);
            }
        }
    }

    // 翻转角色
    private void Flip(float directionX)
    {
        if (directionX > 0 && !facingRight)
        {
            // 朝右
            facingRight = true;
            if (visualTransform != null)
            {
                Vector3 scale = visualTransform.localScale;
                scale.x = Mathf.Abs(scale.x);
                visualTransform.localScale = scale;
            }
        }
        else if (directionX < 0 && facingRight)
        {
            // 朝左
            facingRight = false;
            if (visualTransform != null)
            {
                Vector3 scale = visualTransform.localScale;
                scale.x = -Mathf.Abs(scale.x);
                visualTransform.localScale = scale;
            }
        }
    }

    // 更新可用路点列表
    private void UpdateAvailableWaypoints()
    {
        availableWaypoints.Clear();

        if (currentWaypoint != null)
        {
            availableWaypoints.AddRange(currentWaypoint.connectedWaypoints);
        }

        // 更新所有路点的可点击状态
        UpdateAllWaypointsClickableState();
    }

    // 更新所有路点的可点击状态
    private void UpdateAllWaypointsClickableState()
    {
        // 找到场景中所有路点
        Waypoint2D[] allWaypoints = FindObjectsOfType<Waypoint2D>();

        foreach (var waypoint in allWaypoints)
        {
            // 检查是否在可用路点列表中
            bool isAvailable = availableWaypoints.Contains(waypoint);
            waypoint.SetClickable(isAvailable);
        }
    }

    // 显示可用路点信息
    private void ShowAvailableWaypoints()
    {
        if (availableWaypoints.Count == 0)
        {
            Debug.Log("没有可前往的路点");
            return;
        }

        Debug.Log("=== 可前往的路点 ===");
        for (int i = 0; i < availableWaypoints.Count; i++)
        {
            Debug.Log($"按 {i + 1} - 前往 {availableWaypoints[i].waypointName}");
        }
    }

    // 在Scene视图中显示可用路点
    private void OnDrawGizmos()
    {
        if (currentWaypoint != null && !isMoving)
        {
            Gizmos.color = Color.blue;
            foreach (var waypoint in currentWaypoint.connectedWaypoints)
            {
                if (waypoint != null)
                {
                    Gizmos.DrawLine(transform.position, waypoint.transform.position);
                }
            }
        }

        if (isMoving && targetWaypoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, targetWaypoint.transform.position);
        }
    }
}