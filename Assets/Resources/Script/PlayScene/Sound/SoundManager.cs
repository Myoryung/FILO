using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    AudioSource myAudio;

    public AudioClip Throw;
    public AudioClip Gun_fire;
    public AudioClip Change_character;

    public AudioClip Leader_hurt;
    public AudioClip Hammerman_hurt;
    public AudioClip Stair_changer;
    public AudioClip Air_canuse;
    public AudioClip Grenade;
    public AudioClip Siren;
    public AudioClip WallCrash;

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

    public void PlayThrow()
    {
        myAudio.volume = 0.8f;
        myAudio.PlayOneShot(Throw);
    }
    public void PlayGunFire()
    {
        myAudio.volume = 0.2f;
        myAudio.PlayOneShot(Gun_fire);
    }
    public void PlayChanger_chracter()
    {
        myAudio.volume = 0.25f;
        myAudio.PlayOneShot(Change_character);
    }
    public void PlayLeaderHurt()
    {
        myAudio.volume = 0.6f;
        myAudio.PlayOneShot(Leader_hurt);
    }
    public void PlayHammermanHurt()
    {
        myAudio.volume = 0.4f;
        myAudio.PlayOneShot(Hammerman_hurt);
    }
    public void PlayStairChange()
    {
        myAudio.volume = 0.5f;
        myAudio.PlayOneShot(Stair_changer);
    }
    public void PlayAirCanUse()
    {
        myAudio.volume = 0.8f;
        myAudio.PlayOneShot(Air_canuse);
    }

    public void PlayGrenade()
    {
        myAudio.volume = 0.8f;
        myAudio.PlayOneShot(Grenade);
    }
    public void PlaySiren()
    {
        myAudio.volume = 0.15f;
        myAudio.PlayOneShot(Siren);
    }
    public void PlayWallCrash()
    {
        myAudio.volume = 0.5f;
        myAudio.PlayOneShot(WallCrash);
    }
}
