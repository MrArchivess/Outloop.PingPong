using UnityEngine;

[System.Serializable]
public struct ShotProfile
{
    // Where the ball wants to land (0 = near net, 1 = back of opponent side
    public Vector2 depth01Range;

    // How wide the shot tends to go (0 = center, 1 = edges)
    public Vector2 width01Range;

    // Desired travel time (lower = faster shot feel
    public Vector2 travelTimeRange;

    // Desired arc height above table
    public Vector2 apexHeightRange;

    // How strongly inputDir affects lateral placement
    public float lateralBiasStrength;

    // 0 = very safe, 1 = very aggressive
    public float aggression;

    // Spin flavor (purely aesthetic / secondary for now)
    public float spinStrength;

    public ShotProfile(
        Vector2 depth01Range,
        Vector2 width01Range,
        Vector2 travelTimeRange,
        Vector2 apexHeightRange,
        float lateralBiasStrength,
        float aggression,
        float spinStrength)
    {
        this.depth01Range = depth01Range;
        this.width01Range = width01Range;
        this.travelTimeRange = travelTimeRange;
        this.apexHeightRange = apexHeightRange;
        this.lateralBiasStrength = lateralBiasStrength;
        this.aggression = aggression;
        this.spinStrength = spinStrength;
    }
}
