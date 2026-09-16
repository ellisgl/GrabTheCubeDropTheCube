using UnityEngine;

public class ResetOnOutOfBounds : MonoBehaviour
{
    public Vector3 spawnPosition;
    public float minY = -2f;
    public float maxY = 10f;
    public float maxDistance = 8f;

    void Start()
    {
        if (spawnPosition == Vector3.zero)
            spawnPosition = transform.position;
    }

    void Update()
    {
        if (transform.position.y < minY || transform.position.y > maxY ||
            Mathf.Abs(transform.position.x) > maxDistance ||
            Mathf.Abs(transform.position.z) > maxDistance)
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
}