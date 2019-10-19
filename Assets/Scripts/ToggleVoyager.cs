using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleVoyager : MonoBehaviour
{
    private bool SelectedCamera0;
    public Camera camera0Voyager;
    public GameObject canvasQuiz;
    public Canvas canvas;

    public void ToggleCamera() {
     
        if (SelectedCamera0) {
            SelectedCamera0 = false;
            //canvasQuiz.SetActive(false);
            Camera1Properties();
        }
        else {

            SelectedCamera0 = true;
            Camera0Properties();
            //canvasQuiz.SetActive(true);
        }

    }

    private void Camera1Properties() {
        camera0Voyager.usePhysicalProperties = true;
        camera0Voyager.focalLength = 20.52f;
        camera0Voyager.fieldOfView = 66.68142f;
    }

    private void Camera0Properties() {
        camera0Voyager.usePhysicalProperties = false;
        camera0Voyager.focalLength = 31.17691f;
        camera0Voyager.fieldOfView = 46.82645f;

    }
}
