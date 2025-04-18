using UnityEngine;

public class CustomerMovement : MonoBehaviour
{
    public float moveSpeed = 2f;
    private Vector3 destination;
    private bool isMoving = false;

    public System.Action OnDestinationReached;

    public void MoveTo(Vector3 target)
    {
        destination = target;
        isMoving = true;
    }

    void Update()
    {
        if (!isMoving) return;

        transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, destination) < 0.1f)
        {
            isMoving = false;
            OnDestinationReached?.Invoke();
        }
    }
}
