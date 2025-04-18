using System.Collections;
using UnityEngine;

public class SlidingDoors : MonoBehaviour
{
    public Transform leftDoor;
    public Transform rightDoor;
    public Vector3 leftOpenOffset = new Vector3(-2f, 0, 0);
    public Vector3 rightOpenOffset = new Vector3(2f, 0, 0);
    public float slideDuration = 1f;
    public float stayOpenTime = 3f;

    private Vector3 leftClosedPos;
    private Vector3 rightClosedPos;
    private bool isMoving = false;

    void Start()
    {
        leftClosedPos = leftDoor.position;
        rightClosedPos = rightDoor.position;
    }

    public void TriggerDoors()
    {
        if (!isMoving)
            StartCoroutine(SlideDoors());
    }

    private IEnumerator SlideDoors()
    {
        isMoving = true;
        Vector3 leftTarget = leftClosedPos + leftOpenOffset;
        Vector3 rightTarget = rightClosedPos + rightOpenOffset;

        // Open doors
        yield return MoveDoors(leftClosedPos, leftTarget, rightClosedPos, rightTarget);

        // Wait open
        yield return new WaitForSeconds(stayOpenTime);

        // Close doors
        yield return MoveDoors(leftTarget, leftClosedPos, rightTarget, rightClosedPos);

        isMoving = false;
    }

    private IEnumerator MoveDoors(Vector3 leftStart, Vector3 leftEnd, Vector3 rightStart, Vector3 rightEnd)
    {
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            float t = elapsed / slideDuration;
            leftDoor.position = Vector3.Lerp(leftStart, leftEnd, t);
            rightDoor.position = Vector3.Lerp(rightStart, rightEnd, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        leftDoor.position = leftEnd;
        rightDoor.position = rightEnd;
    }
}
