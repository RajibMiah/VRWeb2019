using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothFollow : MonoBehaviour
{

    public Transform target;

    public float smoothSpeed = 0.125f;
    public Vector3 offset;
    public float distance = 5.0f;
    public float xSpeed = 220.0f;
    public float ySpeed = 120.0f;

    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    public float distanceMin = .5f;
    public float distanceMax = 15f;
    Vector3 velocity = Vector3.zero;

    float x = 0.0f;
    float y = 0.0f;

    private void Start() {
        offset = target.position - transform.position;
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

    }

    void LateUpdate() {

        if (target) {
            // transform.LookAt(target);
            x += Input.GetAxis("Horizontal") * xSpeed * distance * 0.02f;
            y -= Input.GetAxis("Vertical") * ySpeed * 0.02f;
            y = ClampAngle(y, yMinLimit, yMaxLimit);
            Quaternion rotation = Quaternion.Euler(y, x, 0);

            distance = Mathf.Clamp(distance - Input.GetAxis("ZoomOut") * 1, distanceMin, distanceMax);
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * negDistance + target.position;

            transform.rotation = rotation;
            //transform.position = position;
            transform.position = Vector3.Lerp(transform.position, position, smoothSpeed * Time.deltaTime);
           
        }
    }

    public static float ClampAngle(float angle, float min, float max) {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}
