using UnityEngine;
using TMPro;

public class ClickHintBlink : MonoBehaviour
{
    TMP_Text t;
    void Start()
    {
        t = GetComponent<TMP_Text>();
    }

    void Update()
    {
        t.alpha = Mathf.PingPong(Time.time * 2f, 1f);
    }
}
