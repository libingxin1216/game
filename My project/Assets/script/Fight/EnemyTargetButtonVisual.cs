using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyTargetButtonVisual : MonoBehaviour
{
    public Button enemyTargetButton;
    public Image buttonImage;
    public Image highlightEffect; // 可选：高亮效果图片

    [Header("Normal State")]
    public Color normalColor = new Color(0.5f, 0.5f, 0.5f, 0.1f);

    [Header("Selectable State")]
    public Color selectableColor = new Color(1f, 0f, 0f, 0.3f);
    public float pulseSpeed = 2f;
    public float pulseMinAlpha = 0.2f;
    public float pulseMaxAlpha = 0.5f;

    private Coroutine pulseCoroutine;

    void Start()
    {
        if (enemyTargetButton == null)
            enemyTargetButton = GetComponent<Button>();

        if (buttonImage == null)
            buttonImage = GetComponent<Image>();

        // 初始设置为不可点击状态
        SetNormalState();
    }

    void Update()
    {
        // 可以添加其他更新逻辑，如果需要的话
    }

    // 设置为正常状态（不可点击）
    public void SetNormalState()
    {
        if (buttonImage != null)
        {
            buttonImage.color = normalColor;
        }

        // 停止脉动效果
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }

        // 隐藏高亮效果
        if (highlightEffect != null)
        {
            highlightEffect.gameObject.SetActive(false);
        }
    }

    // 设置为可点击状态
    public void SetSelectableState()
    {
        if (buttonImage != null)
        {
            buttonImage.color = selectableColor;
        }

        // 启动脉动效果
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
        }
        pulseCoroutine = StartCoroutine(PulseEffect());

        // 显示高亮效果
        if (highlightEffect != null)
        {
            highlightEffect.gameObject.SetActive(true);
        }
    }

    // 脉动效果
    IEnumerator PulseEffect()
    {
        if (buttonImage == null) yield break;

        while (true)
        {
            float alpha = Mathf.Lerp(pulseMinAlpha, pulseMaxAlpha,
                (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);

            Color color = buttonImage.color;
            color.a = alpha;
            buttonImage.color = color;

            yield return null;
        }
    }

    // 点击反馈效果
    public void PlayClickEffect()
    {
        StartCoroutine(ClickEffect());
    }

    IEnumerator ClickEffect()
    {
        if (buttonImage == null) yield break;

        Color originalColor = buttonImage.color;
        Color flashColor = new Color(1f, 1f, 1f, 0.8f);

        // 快速变白
        float elapsedTime = 0f;
        while (elapsedTime < 0.1f)
        {
            elapsedTime += Time.deltaTime;
            buttonImage.color = Color.Lerp(originalColor, flashColor, elapsedTime / 0.1f);
            yield return null;
        }

        // 快速恢复
        elapsedTime = 0f;
        while (elapsedTime < 0.1f)
        {
            elapsedTime += Time.deltaTime;
            buttonImage.color = Color.Lerp(flashColor, originalColor, elapsedTime / 0.1f);
            yield return null;
        }

        buttonImage.color = originalColor;
    }
}