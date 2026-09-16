using UnityEngine;

public class ClampVelocity : MonoBehaviour
{
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float maxAngularSpeed = 10f;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (rb.linearVelocity.magnitude > maxSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;

        if (rb.angularVelocity.magnitude > maxAngularSpeed)
            rb.angularVelocity = rb.angularVelocity.normalized * maxAngularSpeed;
    }
}
