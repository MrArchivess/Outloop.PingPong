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
        MatchFlowUIController.ReadyMusic += PlayClip;
        MatchFlowUIController.ActiveMusic += PlayLoop;
        MatchFlowUIController.EndMusic += PlayClip;
    }

    private void OnDisable()
    {
        MatchFlowUIController.ReadyMusic -= PlayClip;
        MatchFlowUIController.ActiveMusic -= PlayLoop;
        MatchFlowUIController.EndMusic -= PlayClip;
    }

    private void PlayClip(AudioClip clip)
    {
        if (!_ac || !clip) return;

        _ac.Stop();
        _ac.loop = false;
        _ac.PlayOneShot(clip);
    }

    private void PlayLoop(AudioClip clip)
    {
        if (!_ac || !clip) return;

        _ac.Stop();
        _ac.loop = true;
        _ac.PlayOneShot(clip);
    }
}
