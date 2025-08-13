using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MatchFlowUIController : MonoBehaviour
{
    [SerializeField] private CanvasGroup readyGroup;
    [SerializeField] private RectTransform readyRT;
    [SerializeField] private CanvasGroup setGroup;
    [SerializeField] private RectTransform setRT;
    [SerializeField] private CanvasGroup goGroup;
    [SerializeField] private RectTransform goRT;

    [SerializeField] private CanvasGroup scoreGroup;
    [SerializeField] private RectTransform scoreRT;
    [SerializeField] private TMP_Text scoreText;

    [SerializeField] private float fadeTime = 0.125f;
    [SerializeField] private float popScale = 1.15f;
    [SerializeField] private float scoreDisplayTime = 2f;

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

        GameManager.PointWon += OnPointWon;
    }

    private void OnDisable()
    {
        GameManager.OnMatchReady -= OnReady;
        GameManager.OnMatchGo -= OnGo;
        GameManager.OnMatchStarted -= OnStarted;

        GameManager.PointWon -= OnPointWon;
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

        if (scoreText) scoreText.text = $"{winner} scores!";

        StartSequence(ScoreFlash());
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

    private IEnumerator ScoreFlash()
    {
        yield return Show(scoreGroup, scoreRT);
        float t = 0f;
        while (t < scoreDisplayTime) { t += Time.unscaledDeltaTime; yield return null; }
        yield return Hide(scoreGroup, scoreRT);
    }

    // snappy ease for the pop
    private float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1 + c3 * Mathf.Pow(x - 1, 3) + c1 * Mathf.Pow(x - 1, 2);
    }


}
