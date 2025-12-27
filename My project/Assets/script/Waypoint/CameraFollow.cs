using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("跟随目标")]
    public Transform target; // 玩家对象

    [Header("跟随设置")]
    public Vector2 offset = Vector2.zero; // 相机偏移（X, Y）
    public float followSpeed = 5f; // 跟随平滑速度
    public bool followX = true; // 是否跟随X轴
    public bool followY = true; // 是否跟随Y轴

    [Header("边界限制")]
    public bool useBounds = false; // 是否使用边界限制
    public Vector2 minBounds = new Vector2(-10, -10); // 最小边界
    public Vector2 maxBounds = new Vector2(10, 10); // 最大边界

    [Header("预瞄设置")]
    public bool useLookAhead = false; // 启用预瞄（相机提前看向移动方向）
    public float lookAheadDistance = 2f; // 预瞄距离

    private Vector3 velocity = Vector3.zero;
    private Vector3 lastTargetPosition;

    void Start()
    {
        if (target != null)
        {
            lastTargetPosition = target.position;
        }
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 targetPosition = CalculateTargetPosition();

        // 平滑移动相机
        Vector3 smoothPosition = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            1f / followSpeed
        );

        // 应用边界限制
        if (useBounds)
        {
            smoothPosition.x = Mathf.Clamp(smoothPosition.x, minBounds.x, maxBounds.x);
            smoothPosition.y = Mathf.Clamp(smoothPosition.y, minBounds.y, maxBounds.y);
        }

        transform.position = smoothPosition;

        lastTargetPosition = target.position;
    }

    private Vector3 CalculateTargetPosition()
    {
        Vector3 desiredPosition = target.position;

        // 添加预瞄效果
        if (useLookAhead)
        {
            Vector3 moveDirection = (target.position - lastTargetPosition).normalized;
            desiredPosition += moveDirection * lookAheadDistance;
        }

        // 应用偏移
        desiredPosition.x += offset.x;
        desiredPosition.y += offset.y;

        // 保持相机原有的Z坐标（重要！）
        desiredPosition.z = transform.position.z;

        // 根据设置决定是否跟随某个轴
        if (!followX)
            desiredPosition.x = transform.position.x;
        if (!followY)
            desiredPosition.y = transform.position.y;

        return desiredPosition;
    }

    // 立即移动到目标位置（无平滑）
    public void SnapToTarget()
    {
        if (target != null)
        {
            Vector3 newPos = CalculateTargetPosition();
            transform.position = newPos;
        }
    }

    // 设置边界
    public void SetBounds(Vector2 min, Vector2 max)
    {
        minBounds = min;
        maxBounds = max;
        useBounds = true;
    }

    // 在Scene视图中绘制边界
    private void OnDrawGizmosSelected()
    {
        if (useBounds)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3(
                (minBounds.x + maxBounds.x) / 2,
                (minBounds.y + maxBounds.y) / 2,
                0
            );
            Vector3 size = new Vector3(
                maxBounds.x - minBounds.x,
                maxBounds.y - minBounds.y,
                0
            );
            Gizmos.DrawWireCube(center, size);
        }
    }
}