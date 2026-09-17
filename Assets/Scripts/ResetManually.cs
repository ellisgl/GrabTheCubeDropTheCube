using UnityEngine;

public class ResetManually : MonoBehaviour
{
    public Vector3 spawnPosition;

    void Start()
    {
        if (spawnPosition == Vector3.zero)
            spawnPosition = transform.position;
    }

    void OnButtonPressed()
    {

            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            transform.position = spawnPosition;
    }
}