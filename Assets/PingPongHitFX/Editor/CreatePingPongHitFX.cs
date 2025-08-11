// Auto-generates a PingPong hit particle prefab with textures & materials.
// RP-agnostic: supports Built-in, URP, (basic) HDRP. v2.1 (no runtime script attached in prefab).
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace Outloop.VFX.Editor
{
    public static class PingPongHitFXCreator
    {
        private const string Root = "Assets/Outloop/PingPongHitFX";
        private const string Gen = Root + "/Generated";
        private const string TexDir = Gen + "/Textures";
        private const string MatDir = Gen + "/Materials";
        private const string PrefabDir = Gen + "/Prefabs";
        private const string PrefabPath = PrefabDir + "/PingPongHitFX.prefab";
        private const string ResourcesDir = Gen + "/Resources/OutloopVFX";
        private const string ResourcesPrefabPath = ResourcesDir + "/PingPongHitFX.prefab";

        [MenuItem("Tools/Outloop/Create PingPong Hit FX Prefab")]
        public static void CreatePrefab()
        {
            EnsureDirs();
            var circle = CreateSoftCircleTexture(TexDir + "/soft_circle.png", 256);
            var ring = CreateRingTexture(TexDir + "/ring.png", 256, 0.45f, 0.6f);

            var shader = FindParticlesAdditiveShader();
            if (shader == null)
            {
                Debug.LogError("No suitable particle shader found. Ensure your RP (URP/HDRP) is installed, or Built-in 'Particles' shaders are available.");
                return;
            }

            var sparkMat = new Material(shader) { name = "PP_Sparks_Add" };
            SetTextureAuto(sparkMat, circle);
            ConfigureAsAdditive(sparkMat);
            AssetDatabase.CreateAsset(sparkMat, MatDir + "/PP_Sparks_Add.mat");

            var ringMat = new Material(shader) { name = "PP_Ring_Add" };
            SetTextureAuto(ringMat, ring);
            ConfigureAsAdditive(ringMat);
            AssetDatabase.CreateAsset(ringMat, MatDir + "/PP_Ring_Add.mat");

            // Root GO
            var root = new GameObject("PingPongHitFX");
            root.transform.position = Vector3.zero;

            // Sparks system
            var sparksGO = new GameObject("Sparks");
            sparksGO.transform.SetParent(root.transform, false);
            var sparks = sparksGO.AddComponent<ParticleSystem>();
            var mrSparks = sparksGO.GetComponent<ParticleSystemRenderer>();
            mrSparks.material = sparkMat;
            ConfigureSparks(sparks);

            // Ring system
            var ringGO = new GameObject("Ring");
            ringGO.transform.SetParent(root.transform, false);
            var ringPS = ringGO.AddComponent<ParticleSystem>();
            var mrRing = ringGO.GetComponent<ParticleSystemRenderer>();
            mrRing.renderMode = ParticleSystemRenderMode.Billboard;
            mrRing.material = ringMat;
            ConfigureRing(ringPS);

            // Do NOT add custom scripts in editor-time to avoid "missing script" on save when scripts haven't compiled yet.

            // Save prefabs
            Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Directory.CreateDirectory(Path.GetDirectoryName(ResourcesPrefabPath));
            PrefabUtility.SaveAsPrefabAsset(prefab, ResourcesPrefabPath);

            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Debug.Log("Outloop: Created prefabs at \n- " + PrefabPath + "\n- " + ResourcesPrefabPath);
        }

        private static void EnsureDirs()
        {
            Directory.CreateDirectory(Gen);
            Directory.CreateDirectory(TexDir);
            Directory.CreateDirectory(MatDir);
            Directory.CreateDirectory(PrefabDir);
            Directory.CreateDirectory(ResourcesDir);
            AssetDatabase.Refresh();
        }

        private static Shader FindParticlesAdditiveShader()
        {
            // Built-in
            var s = Shader.Find("Particles/Additive");
            if (s) return s;

            // URP particle shaders
            s = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (s) return s;

            // URP unlit
            s = Shader.Find("Universal Render Pipeline/Unlit");
            if (s) return s;

            // HDRP unlit (basic fallback)
            s = Shader.Find("HDRP/Unlit");
            if (s) return s;

            // Built-in fallback
            s = Shader.Find("Particles/Standard Unlit");
            if (s) return s;

            return null;
        }

        private static void ConfigureAsAdditive(Material m)
        {
            if (!m) return;
            m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            if (m.HasProperty("_Surface")) m.SetFloat("_Surface", 1f); // Transparent
            if (m.HasProperty("_Blend")) m.SetFloat("_Blend", 2f);     // Additive (URP)
            if (m.HasProperty("_ZWrite")) m.SetFloat("_ZWrite", 0f);
            if (m.HasProperty("_Cull")) m.SetFloat("_Cull", 2f);
            if (m.HasProperty("_AlphaClip")) m.SetFloat("_AlphaClip", 0f);

            if (m.HasProperty("_TintColor")) m.SetColor("_TintColor", Color.white);

            m.DisableKeyword("_ALPHATEST_ON");
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            m.EnableKeyword("_ADDITIVE_BLEND");
        }

        private static void SetTextureAuto(Material m, Texture tex)
        {
            if (!m) return;
            if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", tex);
            if (m.HasProperty("_MainTex")) m.SetTexture("_MainTex", tex);
        }

        private static void ConfigureSparks(ParticleSystem ps)
        {
            var main = ps.main;
            main.duration = 0.5f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.18f, 0.32f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2.5f, 4.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.09f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = false;
            main.maxParticles = 64;
            main.gravityModifier = 0f;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            var burst = new ParticleSystem.Burst(0f, 18, 26, 1, 0.01f);
            emission.SetBursts(new ParticleSystem.Burst[] { burst });

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 22f;
            shape.radius = 0.02f;
            shape.arc = 360f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(new Color(1f, 1f, 1f), 0f),
                    new GradientColorKey(new Color(0.65f, 0.9f, 1f), 0.35f),
                    new GradientColorKey(new Color(0.4f, 0.8f, 1f), 1f),
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.8f, 0.2f),
                    new GradientAlphaKey(0.0f, 1f),
                }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve(
                new Keyframe(0f, 0.75f, 0f, 1f),
                new Keyframe(0.2f, 1f),
                new Keyframe(1f, 0f)
            );
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            var trails = ps.trails;
            trails.enabled = true;
            trails.lifetime = 0.12f;
            trails.dieWithParticles = true;
            trails.inheritParticleColor = true;
            trails.ratio = 1f;
        }

        private static void ConfigureRing(ParticleSystem ps)
        {
            var main = ps.main;
            main.duration = 0.5f;
            main.loop = false;
            main.startLifetime = 0.25f;
            main.startSpeed = 0f;
            main.startSize = 0.25f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = false;
            main.maxParticles = 4;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            var burst = new ParticleSystem.Burst(0f, 1, 1, 1, 0.01f);
            emission.SetBursts(new ParticleSystem.Burst[] { burst });

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve(
                new Keyframe(0f, 0.1f),
                new Keyframe(1f, 1.6f)
            );
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(new Color(1f, 1f, 1f), 0f),
                    new GradientColorKey(new Color(0.7f, 0.95f, 1f), 1f),
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(0.9f, 0f),
                    new GradientAlphaKey(0.0f, 1f),
                }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);
        }

        private static Texture2D CreateSoftCircleTexture(string assetPath, int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            float cx = (size - 1) * 0.5f;
            float cy = (size - 1) * 0.5f;
            float maxR = cx;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - cx) / maxR;
                    float dy = (y - cy) / maxR;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Clamp01(1f - Mathf.SmoothStep(0.0f, 1.0f, r));
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            WriteTexture(assetPath, tex);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        }

        private static Texture2D CreateRingTexture(string assetPath, int size, float inner, float outer)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            float cx = (size - 1) * 0.5f;
            float cy = (size - 1) * 0.5f;
            float maxR = cx;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - cx) / maxR;
                    float dy = (y - cy) / maxR;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.SmoothStep(inner, inner - 0.08f, r) * Mathf.SmoothStep(outer, outer + 0.06f, r);
                    a = Mathf.Clamp01(a);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            tex.Apply();
            WriteTexture(assetPath, tex);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        }

        private static void WriteTexture(string assetPath, Texture2D tex)
        {
            var bytes = tex.EncodeToPNG();
            var sysPath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
            Directory.CreateDirectory(Path.GetDirectoryName(sysPath));
            File.WriteAllBytes(sysPath, bytes);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            var ti = (TextureImporter)TextureImporter.GetAtPath(assetPath);
            if (ti != null)
            {
                ti.textureType = TextureImporterType.Default;
                ti.alphaIsTransparency = true;
                ti.mipmapEnabled = false;
                ti.filterMode = FilterMode.Bilinear;
                ti.wrapMode = TextureWrapMode.Clamp;
                ti.SaveAndReimport();
            }
        }
    }
}
#endif
