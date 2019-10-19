using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public void ChangeColor(string color) {
        if(color=="black")
            GetComponent<MeshRenderer>().material.color = Color.black;
        else if (color == "red")
            GetComponent<MeshRenderer>().material.color = Color.red;
        else if (color == "blue")
            GetComponent<MeshRenderer>().material.color = Color.blue;
    }
}
