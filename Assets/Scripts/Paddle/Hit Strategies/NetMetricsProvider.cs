// NetMetricsProvider.cs
using UnityEngine;

[ExecuteAlways]
public sealed class NetMetricsProvider : MonoBehaviour
{
    [Tooltip("If set, this transform’s world Y is used as the top of the net (overrides bounds max).")]
    public Transform topMarkerOverride;

    [Tooltip("Extra clearance to add on top of net (meters).")]
    public float extraMargin = 0.03f;

    [Tooltip("Draw simple gizmos to visualize plane and top height.")]
    public bool drawGizmos = true;

    Bounds worldBounds;

    public bool TryGetMetrics(out float planeZ, out float topY, out Bounds bounds)
    {
        bounds = default;
        planeZ = 0f; topY = 0f;

        // Aggregate child renderers/colliders to get a robust worldBounds
        bool any = false;
        var rends = GetComponentsInChildren<Renderer>(true);
        foreach (var r in rends)
        {
            if (!any) { worldBounds = r.bounds; any = true; }
            else worldBounds.Encapsulate(r.bounds);
        }

        var cols = GetComponentsInChildren<Collider>(true);
        foreach (var c in cols)
        {
            if (!any) { worldBounds = c.bounds; any = true; }
            else worldBounds.Encapsulate(c.bounds);
        }

        if (!any) return false;

        bounds = worldBounds;
        planeZ = worldBounds.center.z;                         // net plane at center Z
        topY = topMarkerOverride ? topMarkerOverride.position.y
                                   : worldBounds.max.y;        // top of tape
        topY += extraMargin;                                  // safety margin
        return true;
    }

    void OnDrawGizmos()
    {
        if (!drawGizmos) return;
        if (!TryGetMetrics(out var z, out var topY, out var b)) return;

        // Plane line
        Gizmos.color = Color.cyan;
        var p1 = new Vector3(b.min.x, topY, z);
        var p2 = new Vector3(b.max.x, topY, z);
        Gizmos.DrawLine(new Vector3(b.min.x, b.min.y, z), new Vector3(b.min.x, b.max.y, z));
        Gizmos.DrawLine(new Vector3(b.max.x, b.min.y, z), new Vector3(b.max.x, b.max.y, z));
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(p1, p2);
    }
}