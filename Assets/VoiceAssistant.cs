using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(AudioSource))]
public class VoiceAssistant : MonoBehaviour
{
    public float StartDelays;
    public float[] delays;
    public AudioClip[] clips;
    private AudioSource audioS;

    void Start()
    {
        audioS = GetComponent<AudioSource>();
        audioS.playOnAwake = false;
        StartCoroutine(StartVoiceClips());
    }

    IEnumerator StartVoiceClips()
    {
        yield return new WaitForSeconds(StartDelays);
        for(int i = 0; i < clips.Length; i++)
        {
            audioS.clip = clips[i];
            audioS.Play();
            yield return new WaitForSeconds(audioS.clip.length + delays[i]);
        }
    }
}
