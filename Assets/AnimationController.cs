using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class AnimationController : MonoBehaviour
{
    public GameObject panelAnimation;
    public GameObject JWT1;
    public GameObject JWT2;
    public MouseOrbitImproved orbit;
    public VRCameraFade fade;
    private Animator anim1, anim2;
    private int currentAnim;
    private bool canClick;

    private float currentClipLength;
    AnimatorClipInfo[] m_AnimatorClipInfo;

    void Start()
    {
        anim1 = JWT1.GetComponent<Animator>();
        anim2 = JWT2.GetComponent<Animator>();
   
        PlayAnimation();
    }

    private void Update() {
      

    }
    public void ClickNext() {
        if (canClick) {
            if (currentAnim > 8) {
                currentAnim = 0;

            }
            else currentAnim++;
            PlayAnimation();
        }
    }

    public void ClickPrevious() {
        if (canClick) {
            if(currentAnim> 0) {
                currentAnim--;
                PlayAnimation();
            }
        }
    }

    private void PlayAnimation() {
        if (currentAnim == 0) {
            JWT1.gameObject.SetActive(true);
            //anim1.Play(0, -1, 0f);
            //anim1.SetInteger("Anim", 0);
            currentClipLength = anim1.GetCurrentAnimatorClipInfo(0).Length;
            StartCoroutine(HandleJWST1());
        }
        else {
            anim2.SetInteger("Anim", currentAnim - 1);
            if (JWT1.activeInHierarchy)
                JWT1.gameObject.SetActive(false);
            StartCoroutine(HandleJWST2(anim2.GetCurrentAnimatorStateInfo(0).length));
        }
    }

    private bool AnimatorIsPlaying(Animator animator) {
        return animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1;
    }

    IEnumerator HandleJWST2(float length) {
        canClick = false;
        panelAnimation.SetActive(false);
        orbit.target = JWT2.transform;
        yield return new WaitForSeconds(length);
        canClick = true;
        panelAnimation.SetActive(true);
    }

    IEnumerator HandleJWST1() {
        canClick = false;
        JWT2.gameObject.SetActive(false);
        orbit.target = JWT1.transform;
        panelAnimation.SetActive(false);
        yield return new WaitForSeconds(anim1.GetCurrentAnimatorStateInfo(0).length);
        fade.FadeIn(false);
        currentAnim = 1;
        ClickNext();
        panelAnimation.SetActive(true);
        canClick = true;
        JWT1.gameObject.SetActive(false);
        JWT2.gameObject.SetActive(true);
        orbit.target = JWT2.transform;
    }

}
