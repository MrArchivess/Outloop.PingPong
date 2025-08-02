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

    private void OnEnable()
    {
        ScoreSystem.scoreUpdated += UpdateScore;
        ScoreSystem.winnerAnnounced += UpdateWin;
    }

    private void OnDisable()
    {
        ScoreSystem.scoreUpdated -= UpdateScore;
        ScoreSystem.winnerAnnounced -= UpdateWin;
    }
}
