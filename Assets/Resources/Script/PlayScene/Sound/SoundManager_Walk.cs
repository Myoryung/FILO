using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager_Walk : MonoBehaviour
{
    public static SoundManager_Walk instance;

    AudioSource myAudio;

    public AudioClip Walk;
    

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    private void Start()
    {
        myAudio = GetComponent<AudioSource>();
    }

    public void PlayWalkSound()
    {
        myAudio.volume = 0.45f;
        myAudio.loop = true;
        myAudio.clip = Walk;
        myAudio.Play();
    }
    public void StopSound()
    {
        myAudio.loop = false;
        myAudio.Stop();
    }
}
