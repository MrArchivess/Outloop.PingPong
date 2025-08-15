using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MatchFlowUIController : MonoBehaviour
{
    [Header("Ready/Set/Go UI")]
    [SerializeField] private CanvasGroup readyGroup;
    [SerializeField] private RectTransform readyRT;
    [SerializeField] private CanvasGroup setGroup;
    [SerializeField] private RectTransform setRT;
    [SerializeField] private CanvasGroup goGroup;
    [SerializeField] private RectTransform goRT;

    [Header("Scoreboard UI")]
    [SerializeField] private CanvasGroup scoreGroup;
    [SerializeField] private RectTransform scoreRT;
    [SerializeField] private TMP_Text winnerText;

    [Header("Scoreboard Tweening")]
    [SerializeField] private float fadeTime = 0.125f;
    [SerializeField] private float popScale = 100f;
    [SerializeField] private float scoreDisplayTime = 2f;

    [Header("Score UI")]
    [SerializeField] private TMP_Text leftScoreTMP;
    [SerializeField] private TMP_Text rightScoreTMP;
    [SerializeField] private RectTransform leftScoreRT;
    [SerializeField] private RectTransform rightScoreRT;

    [Header("Score Tweening")]
    [SerializeField, Range(0.1f, 1.5f)] private float scoreTweenTime = 0.6f;
    [SerializeField, Range(0.1f, 1.5f)] private float scorePunchScale = 1.12f;
    [SerializeField, Range(0.05f, 0.35f)] private float scorePunchTime = 0.15f;

    private Coroutine seq;

    private void Awake()
    {
        HideInstant(readyGroup); HideInstant(goGroup);
        if (setGroup) HideInstant(setGroup);
        if (scoreGroup) HideInstant(scoreGroup);
    }

    private void OnEnable()
    {
        GameManager.OnMatchReady += OnReady;
        GameManager.OnMatchGo += OnGo;
        GameManager.OnMatchStarted += OnStarted;

        ScoreSystem.ScoreUpdated += OnPointWon;
    }

    private void OnDisable()
    {
        GameManager.OnMatchReady -= OnReady;
        GameManager.OnMatchGo -= OnGo;
        GameManager.OnMatchStarted -= OnStarted;

        ScoreSystem.ScoreUpdated -= OnPointWon;
    }

    private void OnReady()
    {
        StartSequence(
            Show(readyGroup, readyRT),
            Hide(goGroup, goRT),
            setGroup ? Hide(setGroup, setRT) : null
            );
    }

    public void OnSet()
    {
        if (!setGroup) return;
        StartSequence(
            Hide(readyGroup, readyRT),
            Show(setGroup, setRT)
            );
    }

    private void OnGo()
    {
        StartSequence(
            Hide(readyGroup, readyRT),
            setGroup ? Hide(setGroup, setRT) : null,
            Show(goGroup, goRT)
            );
    }
    private void OnStarted()
    {
        StartSequence(
            Hide(readyGroup, readyRT),
            setGroup ? Hide(setGroup, setRT) : null,
            Hide(goGroup, goRT)
            );
    }

    private void OnPointWon(PlayerSide winner)
    {
        if (!scoreGroup) return;

        if (winnerText) winnerText.text = $"{winner} scores!";

        int leftOld = ScoreSystem.Instance.ScoreLeft;
        int rightOld = ScoreSystem.Instance.ScoreRight;

        int leftNew = leftOld + (winner == PlayerSide.Left ? 1 : 0);
        int rightNew = rightOld + (winner == PlayerSide.Right ? 1 : 0);

        StartSequence(ScoreFlash(leftOld, rightOld, leftNew, rightNew, winner));
    }

    private void StartSequence(params IEnumerator[] steps)
    {
        if (seq != null) StopCoroutine(seq);
        seq = StartCoroutine(Run());
        IEnumerator Run()
        {
            foreach (var step in steps)
            {
                if (step == null) continue;
                yield return StartCoroutine(step);
            }
        }

    }

    private IEnumerator Show(CanvasGroup g, RectTransform rt)
    {
        if (!g) yield break;
        g.gameObject.SetActive(true);
        g.interactable = false; g.blocksRaycasts = false;
        float t = 0f; g.alpha = 0f;
        Vector3 from = Vector3.one / popScale, to = Vector3.one;

        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeTime);
            g.alpha = k;
            rt.localScale = Vector3.Lerp(from, to, EaseOutBack(k));
            yield return null;
        }
        g.alpha = 1f; rt.localScale = to;
    }

    private IEnumerator Hide(CanvasGroup g, RectTransform rt)
    {
        if (!g) yield break;
        float t = 0f; float start = g.alpha;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeTime);
            g.alpha = Mathf.Lerp(start, 0f, k);
            yield return null;
        }
        g.alpha = 0f;
        g.gameObject.SetActive(false);
    }

    private void HideInstant(CanvasGroup g)
    {
        if (!g) return;
        g.alpha = 0f;
        g.gameObject.SetActive(false);
    }

    private IEnumerator ScoreFlash(int leftOld, int rightOld, int leftNew, int rightNew, PlayerSide winner)
    {
        if (leftScoreTMP) leftScoreTMP.text = leftOld.ToString();
        if (rightScoreTMP) rightScoreTMP.text = rightOld.ToString();

        yield return Show(scoreGroup, scoreRT);

        if (winner == PlayerSide.Left)
        {
            if (leftScoreTMP) yield return TweenScore(leftScoreTMP, leftOld, leftNew, scoreTweenTime);
            if (leftScoreRT) yield return Punch(leftScoreRT, scorePunchScale, scorePunchTime);
        }
        else
        {
            if (rightScoreTMP) yield return TweenScore(rightScoreTMP, rightOld, rightNew, scoreTweenTime);
            if (rightScoreRT) yield return Punch(rightScoreRT, scorePunchScale, scorePunchTime);  
        }

        float t = 0f;

        while (t < scoreDisplayTime) { t += Time.unscaledDeltaTime; yield return null; }

        yield return Hide(scoreGroup, scoreRT);
    }

    private IEnumerator TweenScore(TMP_Text tmp, int from, int to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            float e = 1f - Mathf.Pow(1f - k, 3f);
            int value = Mathf.RoundToInt(Mathf.Lerp(from, to, e));
            tmp.text = value.ToString();
            yield return null;
        }
        tmp.text = to.ToString();
    }

    private IEnumerator Punch(RectTransform target, float scale, float dur)
    {
        if (!target) yield break;
        Vector3 s0 = target.localScale;
        Vector3 s1 = s0 * scale;

        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            target.localScale = Vector3.Lerp(s0, s1, t / dur);
            yield return null;
        }
        t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            target.localScale = Vector3.Lerp(s1, s0, t / dur);
            yield return null;
        }
        target.localScale = s0;
    }

    // snappy ease for the pop
    private float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1 + c3 * Mathf.Pow(x - 1, 3) + c1 * Mathf.Pow(x - 1, 2);
    }


}
