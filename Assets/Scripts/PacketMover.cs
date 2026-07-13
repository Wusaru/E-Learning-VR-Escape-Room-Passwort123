//Tommy Bui

using UnityEngine;
using System;
public class PacketMover : MonoBehaviour
{
    public Transform target;
    public float speed = 2f;
    public bool destroyOnArrival = true;

    public Action<GameObject> OnArrived;

    void Update()
    {
        if (target == null) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            OnArrived?.Invoke(gameObject);

            if (destroyOnArrival)
            {
                Destroy(gameObject);
            }
        }
    }
}