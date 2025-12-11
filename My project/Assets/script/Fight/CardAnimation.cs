using UnityEngine;
using UnityEngine.UI;

public class CardAnimation : MonoBehaviour
{
    private Vector3 originalScale;
    private Button button;

    void Start()
    {
        originalScale = transform.localScale;
        button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.AddListener(OnClickAnimation);
        }
    }

    void OnClickAnimation()
    {
        StartCoroutine(ClickAnimation());
    }

    System.Collections.IEnumerator ClickAnimation()
    {
        transform.localScale = originalScale * 0.8f;
        yield return new WaitForSeconds(0.1f);
        transform.localScale = originalScale;
    }
}