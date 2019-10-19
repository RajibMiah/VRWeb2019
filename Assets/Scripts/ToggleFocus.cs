using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleFocus : MonoBehaviour
{
    private MouseOrbitImproved mouseOrbit;
    private bool isFocused = true;
    private Vector3 originalPos;
    private Quaternion originalRot;
    private Vector3 targetPos;
    private Quaternion targetRot;
    public  float speed = 5;
    void Start()
    {
        mouseOrbit = GetComponent<MouseOrbitImproved>();
        originalRot = transform.localRotation;
        originalPos = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Click") || Input.GetKeyDown(KeyCode.Escape)) {
            Toggle();
        }
        if (!isFocused) {
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPos, speed * Time.deltaTime);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, originalRot, speed * Time.deltaTime);
     
        }
    }

    public void Toggle() {
        isFocused = !isFocused;
        if(!isFocused && mouseOrbit.enabled) {
            mouseOrbit.enabled = false;
            transform.localRotation =  originalRot;
            transform.localPosition = originalPos;
        }
        else {
            mouseOrbit.enabled = true;
        }
    }
}
