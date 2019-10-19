using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class Voyger1SpeedControl : MonoBehaviour
{
    //public float WheninTheCollider;
    public float whenOutOfCollider;



    public PathCreator pathCreator;
    public EndOfPathInstruction endOfPathInstruction;
    public float speed;
    float distanceTravelled;
    //for canvas position for quizs
    public Transform CanvasContainer;
    public Transform transformForEarth, transformForMars, transformForJupiter, transformForSaturn, transformForUranus, transformForNeptune;

    private void Start()
    {
        var boxCollider = gameObject.AddComponent<BoxCollider>();
        speed = 0f;

    }

    void Update()
    {
        if (pathCreator != null)
        {
            distanceTravelled += speed * Time.deltaTime;
            transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
            // transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);
        }
    }
    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.tag == "Earth")
        {
            speed = 1f;
            CanvasContainer.transform.position = transformForEarth.transform.position;
            CanvasContainer.transform.rotation = transformForEarth.transform.rotation;
            
            //Debug.Log("earth speed" + speed);
        }
        else if (other.gameObject.tag == "Mars")
        {
            speed = 0.05f;
            CanvasContainer.transform.position = transformForMars.transform.position;
            CanvasContainer.transform.rotation = transformForMars.transform.rotation;
        }
        else if (other.gameObject.tag == "Jupiter")
        {
            speed = 2f;
            CanvasContainer.transform.position = transformForJupiter.transform.position;
            CanvasContainer.transform.rotation = transformForJupiter.transform.rotation;
            // Debug.Log("jupiter speed" + speed);
        }
        else if (other.gameObject.tag == "Saturn")
        {
            speed = 1f;

            CanvasContainer.transform.position = transformForSaturn.transform.position;
            CanvasContainer.transform.rotation = transformForSaturn.transform.rotation;
            // Debug.Log("Saturn speed" + speed);
        }
        else if (other.gameObject.tag == "Uranus")
        {
            speed = 1f;
            CanvasContainer.transform.position = transformForUranus.transform.position;
            CanvasContainer.transform.rotation = transformForUranus.transform.rotation;
            // Debug.Log("Uranus speed" + speed);
        }
        else if (other.gameObject.tag == "Neptune")
        {
            speed = 1f;
            CanvasContainer.transform.position = transformForNeptune.transform.position;
            CanvasContainer.transform.rotation = transformForNeptune.transform.rotation;
            // Debug.Log("Neptune speed" + speed);
        }
    }


    private void OnTriggerExit(Collider other)
    {
        speed = whenOutOfCollider;
    }

}
