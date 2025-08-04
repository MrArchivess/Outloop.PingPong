using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreUIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text leftPlayerText;
    [SerializeField] private TMP_Text leftPlayerScore;

    [SerializeField] private TMP_Text rightPlayerText;
    [SerializeField] private TMP_Text rightPlayerScore;

    [SerializeField] private ScoreSystem scoreSystem;

    private void UpdateScore(PlayerSide scoredPlayer)
    {
        if (scoredPlayer == PlayerSide.Left) leftPlayerScore.text = scoreSystem.ScoreLeft.ToString();
        else rightPlayerScore.text = scoreSystem.ScoreRight.ToString();
    }

    private void UpdateWin(PlayerSide winner)
    {
        if (winner == PlayerSide.Left) leftPlayerText.color = Color.red;
        else rightPlayerText.color = Color.red;
    }

    private void ResetScoreText()
    {
        leftPlayerScore.text = "0";
        leftPlayerText.color = Color.black;

        rightPlayerScore.text = "0";
        rightPlayerScore.color = Color.black;
    }

    private void OnEnable()
    {
        ScoreSystem.ScoreUpdated += UpdateScore;
        ScoreSystem.WinnerAnnounced += UpdateWin;
        GameManager.OnMatchReset += ResetScoreText;
    }

    private void OnDisable()
    {
        ScoreSystem.ScoreUpdated -= UpdateScore;
        ScoreSystem.WinnerAnnounced -= UpdateWin;
        GameManager.OnMatchReset -= ResetScoreText;
    }
}
