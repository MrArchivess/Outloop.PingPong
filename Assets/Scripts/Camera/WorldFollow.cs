using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 0.3f, 0);
    [SerializeField] private SpriteRenderer sr;

    private bool _isServing;

    private void Awake()
    {
        if (!sr) sr = GetComponent<SpriteRenderer>();
        Hide();
    }

    private void LateUpdate()
    {
        if (!_isServing || !target) return;
        transform.position = target.position + offset;
    }

    private void OnEnable()
    {
        GameManager.OnServerDetermined += HandleServerDetermined;
        HitDetector.OnServeCompleted += HandleServeCompleted;

        GameManager.OnMatchReady += Hide;
        GameManager.OnMatchStarted += Hide;
        GameManager.OnMatchReset += Hide;
        MatchFlowUIController.MatchFinished += Hide;
        ScoreSystem.WinnerAnnounced += _ => Hide();
        //BallController.OnBallReset += Hide; // optional
    }

    private void OnDisable()
    {
        GameManager.OnServerDetermined -= HandleServerDetermined;
        HitDetector.OnServeCompleted -= HandleServeCompleted;

        GameManager.OnMatchReady -= Hide;
        GameManager.OnMatchStarted -= Hide;
        GameManager.OnMatchReset -= Hide;
        MatchFlowUIController.MatchFinished -= Hide;
        ScoreSystem.WinnerAnnounced -= _ => Hide();
        //BallController.OnBallReset -= Hide;
    }

    private void HandleServerDetermined(GameObject serverGO)
    {
        target = serverGO ? serverGO.transform : null;
        Show();
    }

    private void HandleServeCompleted() => Hide();

    private void Show()
    {
        _isServing = true;
        if (sr) sr.enabled = true;
        //enabled = true;
    }

    private void Hide()
    {
        _isServing = false;
        if (sr) sr.enabled = false;
        //enabled = false;
    }

}
