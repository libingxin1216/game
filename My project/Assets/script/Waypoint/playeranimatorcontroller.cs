using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator2D : MonoBehaviour
{
    [Header("组件引用")]
    public PlayerWaypointController2D waypointController;
    private Animator animator;

    [Header("动画参数名称")]
    public string isMovingParam = "IsMoving";
    public string speedParam = "Speed";

    [Header("调试")]
    public bool showDebugInfo = true;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (waypointController == null)
        {
            // 尝试从父对象查找
            waypointController = GetComponentInParent<PlayerWaypointController2D>();

            // 如果还是没找到，尝试从场景中查找
            if (waypointController == null)
            {
                waypointController = FindObjectOfType<PlayerWaypointController2D>();
            }
        }

        // 验证
        if (animator == null)
        {
            Debug.LogError("未找到Animator组件！");
            return;
        }

        if (waypointController == null)
        {
            Debug.LogError("未找到PlayerWaypointController2D！");
            return;
        }

        // 检查Animator Controller
        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning("Animator没有设置Controller！动画将不会播放。");
        }

        if (showDebugInfo)
        {
            Debug.Log($"=== PlayerAnimator2D 初始化 ===");
            Debug.Log($"Animator: {(animator != null ? "✓" : "✗")}");
            Debug.Log($"Controller: {(waypointController != null ? "✓" : "✗")}");
            Debug.Log($"Has Controller: {(animator.runtimeAnimatorController != null ? "✓" : "✗")}");
            Debug.Log($"Has Parameter '{isMovingParam}': {HasParameter(isMovingParam)}");
        }
    }

    void Update()
    {
        if (animator == null || waypointController == null)
            return;

        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        bool isMoving = waypointController.isMoving;

        // 更新IsMoving参数
        if (HasParameter(isMovingParam))
        {
            animator.SetBool(isMovingParam, isMoving);

            if (showDebugInfo && Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log($"设置 {isMovingParam} = {isMoving}");
            }
        }
        else if (showDebugInfo)
        {
            Debug.LogWarning($"Animator没有参数: {isMovingParam}");
            showDebugInfo = false; // 只警告一次
        }

        // 更新Speed参数（如果存在）
        if (HasParameter(speedParam))
        {
            float speed = isMoving ? 1f : 0f;
            animator.SetFloat(speedParam, speed);
        }
    }

    // 检查参数是否存在
    private bool HasParameter(string paramName)
    {
        if (animator == null || string.IsNullOrEmpty(paramName))
            return false;

        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }

    // 手动播放动画（用于测试）
    public void PlayAnimation(string animationName)
    {
        if (animator != null)
        {
            animator.Play(animationName);
            Debug.Log($"播放动画: {animationName}");
        }
    }

    // 设置动画速度
    public void SetAnimationSpeed(float speed)
    {
        if (animator != null)
        {
            animator.speed = speed;
        }
    }

    // 测试动画系统（在Inspector中右键点击）
    [ContextMenu("测试 - 播放移动")]
    private void TestMoving()
    {
        if (animator != null && HasParameter(isMovingParam))
        {
            animator.SetBool(isMovingParam, true);
            Debug.Log("手动设置IsMoving = true");
        }
    }

    [ContextMenu("测试 - 停止移动")]
    private void TestIdle()
    {
        if (animator != null && HasParameter(isMovingParam))
        {
            animator.SetBool(isMovingParam, false);
            Debug.Log("手动设置IsMoving = false");
        }
    }

    [ContextMenu("显示所有参数")]
    private void ShowAllParameters()
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning("没有Animator Controller");
            return;
        }

        Debug.Log("=== Animator 参数列表 ===");
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            Debug.Log($"- {param.name} ({param.type})");
        }
    }
}