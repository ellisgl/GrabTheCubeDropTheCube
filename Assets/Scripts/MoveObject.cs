using UnityEngine;

public class MoveObject : MonoBehaviour
{
    public Rigidbody targetRigidbody; // We need something to move.
    public Vector3 spawnPosition; // The position to reset to.

    void Start()
    {
        if (spawnPosition == Vector3.zero && targetRigidbody != null)
            spawnPosition = targetRigidbody.transform.position;
    }

    public void OnButtonPressed()
    {
        if (targetRigidbody != null)
        {
            // If we didn't messup linking the Rigidbody, reset its position and velocity.
            targetRigidbody.linearVelocity = Vector3.zero;
            targetRigidbody.angularVelocity = Vector3.zero;
            targetRigidbody.transform.position = spawnPosition;
        }
    }
}