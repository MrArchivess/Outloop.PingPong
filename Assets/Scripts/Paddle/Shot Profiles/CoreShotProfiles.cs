using UnityEngine;

public static class CoreShotProfiles
{
    public static ShotProfile Drive => new ShotProfile(
        depth01Range: new Vector2(0.40f, 0.60f),
        width01Range: new Vector2(0.30f, 0.70f),
        travelTimeRange: new Vector2(0.60f, 0.90f),
        apexHeightRange: new Vector2(0.40f, 0.70f),
        lateralBiasStrength: 1.00f,
        aggression: 0.30f,
        spinStrength: 0.50f
    );

    public static ShotProfile Quick => new ShotProfile(
        depth01Range: new Vector2(0.45f, 0.65f),
        width01Range: new Vector2(0.40f, 0.80f),
        travelTimeRange: new Vector2(0.40f, 0.60f),
        apexHeightRange: new Vector2(0.25f, 0.45f),
        lateralBiasStrength: 1.00f,
        aggression: 0.60f,
        spinStrength: 0.60f
    );

    public static ShotProfile Lob => new ShotProfile(
    depth01Range: new Vector2(0.70f, 0.95f),
    width01Range: new Vector2(0.20f, 0.80f),
    travelTimeRange: new Vector2(1.00f, 1.40f),
    apexHeightRange: new Vector2(1.00f, 1.60f),
    lateralBiasStrength: 0.80f,
    aggression: 0.10f,
    spinStrength: 0.40f
    );

    public static ShotProfile Power => new ShotProfile(
    depth01Range: new Vector2(0.60f, 0.85f),
    width01Range: new Vector2(0.40f, 0.90f),
    travelTimeRange: new Vector2(0.35f, 0.50f),
    apexHeightRange: new Vector2(0.20f, 0.40f),
    lateralBiasStrength: 1.10f,
    aggression: 0.90f,
    spinStrength: 1.00f
    );

    public static ShotProfile Drop => new ShotProfile(
    depth01Range: new Vector2(0.05f, 0.25f),
    width01Range: new Vector2(0.30f, 0.70f),
    travelTimeRange: new Vector2(0.50f, 0.80f),
    apexHeightRange: new Vector2(0.30f, 0.60f),
    lateralBiasStrength: 0.90f,
    aggression: 0.50f,
    spinStrength: 0.70f
    );
}
