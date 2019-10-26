using UnityEngine;
using System.Collections;

public class SmoothFollow2 : MonoBehaviour
{
    
    public Transform target;
    public float speed = 5;
    private Vector3 offset;

    private void Start()
    {
       // offset = transform.position + target.position;
    }

    protected void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, target.position + offset, speed * Time.deltaTime);
    }
}