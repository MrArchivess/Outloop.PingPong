using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchFlowUIController : MonoBehaviour
{
    [SerializeField] private CanvasGroup readyGroup;
    [SerializeField] private RectTransform readyRT;
    [SerializeField] private CanvasGroup goGroup;
    [SerializeField] private RectTransform goRT;

    [SerializeField] private float fadeTime = 0.125f;
    [SerializeField] private float popScale = 1.15f;

    private Coroutine current;

    private void Awake() => TransitionToStarted();

    private void TransitionToReady()
    {
        StartSeq(ref current, Show(readyGroup, readyRT), Hide(goGroup, goRT));
    }
    private void TransitionToGo()
    {
        StartSeq(ref current, Show(goGroup, goRT), Hide(readyGroup, readyRT));
    }
    private void TransitionToStarted()
    {
        StartSeq(ref current, Hide(readyGroup, readyRT), Hide(goGroup, goRT));
    }

    private void StartSeq(ref Coroutine handle, IEnumerator a, IEnumerator b = null)
    {
        if (handle != null) StopCoroutine(handle);
        handle = StartCoroutine(Seq());

        IEnumerator Seq()
        {
            yield return StartCoroutine(a);
            if (b != null) yield return StartCoroutine(b);
        }
    }

    private IEnumerator Show(CanvasGroup g, RectTransform rt)
    {
        g.gameObject.SetActive(true);
        rt.localScale = Vector3.one * (1f / popScale);
        g.alpha = 0f;

        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeTime);
            g.alpha = k;
            rt.localScale = Vector3.Lerp(Vector3.one / popScale, Vector3.one, EaseOutBack(k));
            yield return null;
        }
        g.alpha = 1f; rt.localScale = Vector3.one;
        g.interactable = false; g.blocksRaycasts = false;
    }

    private IEnumerator Hide(CanvasGroup g, RectTransform rt)
    {
        float t = 0f;
        float start = g.alpha;
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

    // snappy ease for the pop
    private float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1 + c3 * Mathf.Pow(x - 1, 3) + c1 * Mathf.Pow(x - 1, 2);
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
