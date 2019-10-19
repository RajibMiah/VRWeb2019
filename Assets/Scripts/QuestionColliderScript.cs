using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestionColliderScript : MonoBehaviour
{
    Voyger1SpeedControl ctrl1;
    Voyger2SpeedCrontroll ctrl2;

    private void Start() {
        ctrl1 = GetComponent<Voyger1SpeedControl>();
        ctrl2 = GetComponent<Voyger2SpeedCrontroll>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "questionCollider")
        {

            //FindObjectOfType<Voyger1SpeedControl>().speed = 0;
            if (ctrl1 != null) {
                ctrl1.speed = 0;
                //ctrl2.enabled = false;
            }
           
            if (ctrl2 != null) {
                ctrl2.speed = 0;
                //ctrl2.enabled = false;
            }
            
            //IF QUESTION IS TRUE THEN GO AHARD AND CONVERT THE SPEED  0 TO 100 
            //IF QUESTION IS NOT TRUE THEN GO BACK TO THE FAST PLANET AND CONVERT THE SPEED 0 TO 0.2
      
     
        }
    }



}
