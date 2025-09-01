using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[DisallowMultipleComponent]
public class MusicManager : MonoBehaviour
{
    private AudioSource _ac;

    private void Awake()
    {
        _ac = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        MatchFlowUIController.OnReadyStarted += PlayClip;
        MatchFlowUIController.OnGoEnded += PlayLoop;
    }

    private void OnDisable()
    {
        MatchFlowUIController.OnReadyStarted -= PlayClip;
        MatchFlowUIController.OnGoEnded -= PlayLoop;
    }

    private void PlayClip(AudioClip clip)
    {
        if (!_ac || !clip) return;

        _ac.Stop();
        _ac.PlayOneShot(clip);
        _ac.loop = false;
    }

    private void PlayLoop(AudioClip clip)
    {
        if (!_ac || !clip) return;

        _ac.Stop();
        _ac.PlayOneShot(clip);
        _ac.loop = true;
    }
}
