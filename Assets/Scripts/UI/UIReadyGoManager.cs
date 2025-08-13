using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIReadyGoManager : MonoBehaviour
{
    [SerializeField] private Image readyImage;
    [SerializeField] private Image goImage;

    private void TransitionToReady()
    {
        readyImage.enabled = true;
        goImage.enabled = false;
    }

    private void TransitionToGo()
    {
        readyImage.enabled = false;
        goImage.enabled = true;
    }

    private void TransitionToStarted()
    {
        readyImage.enabled = false;
        goImage.enabled = false;
    }

    private void OnEnable()
    {
        GameManager.OnMatchReady += TransitionToReady;
        GameManager.OnMatchGo += TransitionToGo;
        GameManager.OnMatchStarted += TransitionToStarted;
    }

    private void OnDisable()
    {
        GameManager.OnMatchReady -= TransitionToReady;
        GameManager.OnMatchGo -= TransitionToGo;
        GameManager.OnMatchStarted -= TransitionToStarted;
    }
}
