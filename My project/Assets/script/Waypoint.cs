using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [Header("与本路点相连的路点")]
    public List<Waypoint> connectedWaypoints = new List<Waypoint>();

    [Header("用于点击检测")]
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;

    private SpriteRenderer sr;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = normalColor;
    }

    private void OnMouseEnter()
    {
        if (sr != null) sr.color = highlightColor;
    }

    private void OnMouseExit()
    {
        if (sr != null) sr.color = normalColor;
    }
}
