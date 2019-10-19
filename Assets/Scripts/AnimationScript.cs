using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationScript : MonoBehaviour
{
    public float speed;
    public float lastSpeed;
    Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        lastSpeed = speed;
    }

    // Update is called once per frame
    void Update()
    {
        if(lastSpeed != speed) {
            anim.speed = speed;
            lastSpeed = speed;
        }
    }
}
