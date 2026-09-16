using UnityEngine;

public class SpinObject : MonoBehaviour
{
    [SerializeField] private Vector3 axis = Vector3.up;
    [SerializeField] private float speed = 360f;

    private void Update()
    {
        transform.Rotate(axis * speed * Time.deltaTime, Space.Self);
    }
}
