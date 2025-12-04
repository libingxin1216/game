using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    public Waypoint currentWaypoint;

    private Waypoint targetWaypoint;
    private bool isMoving = false;

    private Vector3 startPoint;
    private Vector3 endPoint;
    private float moveProgress = 0f;

    private void Start()
    {
        if (currentWaypoint != null)
        {
            transform.position = currentWaypoint.transform.position;
        }
    }

    void Update()
    {
        HandleClick();

        if (isMoving)
        {
            MoveOnLine();
        }
    }

    void HandleClick()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mousePos);
        if (hit == null) return;

        Waypoint clicked = hit.GetComponent<Waypoint>();
        if (clicked == null) return;

        if (currentWaypoint.connectedWaypoints.Contains(clicked))
        {
            targetWaypoint = clicked;
            StartMoving();
        }
    }

    void StartMoving()
    {
        startPoint = currentWaypoint.transform.position;
        endPoint = targetWaypoint.transform.position;
        moveProgress = 0f;
        isMoving = true;
    }

    void MoveOnLine()
    {
        moveProgress += Time.deltaTime * (moveSpeed / Vector3.Distance(startPoint, endPoint));

        transform.position = Vector3.Lerp(startPoint, endPoint, moveProgress);

        if (moveProgress >= 1f)
        {
            transform.position = endPoint;
            currentWaypoint = targetWaypoint;
            targetWaypoint = null;
            isMoving = false;
        }
    }
}

