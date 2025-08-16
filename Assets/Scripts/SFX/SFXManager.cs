using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[DisallowMultipleComponent]
public class SFXManager : MonoBehaviour
{
    private AudioSource _ac;

    private void Awake()
    {
        _ac = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        HitDetector.OnBallHitSFX += PlayClip;
        BallController.OnBallTableBounce += PlayClip;
    }

    private void OnDisable()
    {
        HitDetector.OnBallHitSFX -= PlayClip;           // ✅ correct unsubscribe
        BallController.OnBallTableBounce -= PlayClip;   // ✅ already correct
    }

    private void PlayClip(AudioClip clip)
    {
        if (!_ac || !clip) return;

        // Randomize pitch a bit for variety; keep range tight to avoid artifacts
        _ac.pitch = Random.Range(0.95f, 1.10f);

        // PlayOneShot doesn’t use _ac.clip; it mixes 'clip' directly
        _ac.PlayOneShot(clip);
    }
}
