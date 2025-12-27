using UnityEngine;

public class SimpleTrainAnimation2D : MonoBehaviour
{
    [Header("组件引用")]
    public PlayerWaypointController2D waypointController;
    public Transform leftWheel;  // 左轮
    public Transform rightWheel; // 右轮
    public Transform body;       // 车身

    [Header("轮子动画设置")]
    public float wheelRotationSpeed = 360f; // 轮子旋转速度（度/秒）

    [Header("车身动画设置")]
    public bool enableBodyBob = true;       // 启用车身上下浮动
    public float bodyBobAmount = 0.05f;     // 浮动幅度
    public float bodyBobSpeed = 8f;         // 浮动速度

    [Header("烟雾效果")]
    public ParticleSystem smokeEffect;      // 烟雾粒子系统

    private float animationTime = 0f;
    private Vector3 bodyOriginalPos;

    void Start()
    {
        if (waypointController == null)
        {
            waypointController = GetComponent<PlayerWaypointController2D>();
        }

        if (body != null)
        {
            bodyOriginalPos = body.localPosition;
        }
    }

    void Update()
    {
        if (waypointController == null)
            return;

        if (waypointController.isMoving)
        {
            // 移动时播放动画
            animationTime += Time.deltaTime;

            AnimateWheels();

            if (enableBodyBob && body != null)
            {
                AnimateBody();
            }

            // 启用烟雾效果
            if (smokeEffect != null && !smokeEffect.isPlaying)
            {
                smokeEffect.Play();
            }
        }
        else
        {
            // 停止时恢复
            if (body != null)
            {
                body.localPosition = Vector3.Lerp(
                    body.localPosition,
                    bodyOriginalPos,
                    Time.deltaTime * 5f
                );
            }

            // 停止烟雾效果
            if (smokeEffect != null && smokeEffect.isPlaying)
            {
                smokeEffect.Stop();
            }
        }
    }

    private void AnimateWheels()
    {
        float rotation = wheelRotationSpeed * Time.deltaTime;

        if (leftWheel != null)
        {
            leftWheel.Rotate(0, 0, -rotation); // Z轴旋转（2D）
        }

        if (rightWheel != null)
        {
            rightWheel.Rotate(0, 0, -rotation);
        }
    }

    private void AnimateBody()
    {
        // 车身轻微上下浮动
        float bobOffset = Mathf.Sin(animationTime * bodyBobSpeed) * bodyBobAmount;
        Vector3 newPos = bodyOriginalPos;
        newPos.y += bobOffset;
        body.localPosition = newPos;
    }
}
