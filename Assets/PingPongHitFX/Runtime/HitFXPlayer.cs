using UnityEngine;
using System.Collections;

namespace Outloop.VFX
{
    /// <summary>
    /// Helper to spawn and orient the PingPongHitFX prefab.
    /// </summary>
    public static class HitFXPlayer
    {
        /// <summary>
        /// Spawns the effect at position, oriented such that its forward faces along 'normal'.
        /// Optionally tints the materials (works for the default generated materials).
        /// Adds an auto-destroy if not present.
        /// </summary>
        public static GameObject Spawn(Vector3 position, Vector3 normal, Color? tint = null)
        {
            var fxPrefab = Resources.Load<GameObject>("OutloopVFX/PingPongHitFX");
            if (fxPrefab == null)
            {
                Debug.LogWarning("PingPongHitFX prefab not found in Resources. Run Tools → Outloop → Create PingPong Hit FX Prefab.");
                return null;
            }

            var go = Object.Instantiate(fxPrefab);
            go.transform.position = position;
            if (normal.sqrMagnitude > 0.0001f)
                go.transform.rotation = Quaternion.LookRotation(normal);

            if (tint.HasValue)
            {
                var rends = go.GetComponentsInChildren<ParticleSystemRenderer>();
                foreach (var r in rends)
                {
                    if (r.sharedMaterial != null)
                    {
                        var mat = Object.Instantiate(r.sharedMaterial);
                        var c = tint.Value;
                        if (mat.HasProperty("_TintColor")) mat.SetColor("_TintColor", c);
                        if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);
                        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
                        r.material = mat;
                    }
                }
            }

            var pss = go.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in pss) ps.Play(true);

            // Ensure auto-destroy at runtime without storing a script reference in the prefab.
            if (go.GetComponent<PingPongHitAutoDestroy>() == null)
                go.AddComponent<PingPongHitAutoDestroy>();

            return go;
        }
    }

    /// <summary>
    /// Destroys the effect after all particles have finished.
    /// </summary>
    public class PingPongHitAutoDestroy : MonoBehaviour
    {
        private float _maxLifetime;

        private void Awake()
        {
            var ps = GetComponentsInChildren<ParticleSystem>(true);
            foreach (var p in ps)
            {
                var main = p.main;
                _maxLifetime = Mathf.Max(_maxLifetime, main.startLifetime.constantMax + main.duration + 0.1f);
            }
            StartCoroutine(Co_Destroy());
        }

        private IEnumerator Co_Destroy()
        {
            yield return new WaitForSeconds(_maxLifetime);
            Destroy(gameObject);
        }
    }
}
