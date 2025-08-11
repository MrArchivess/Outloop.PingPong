# Outloop PingPong Hit FX (v2, URP/HDRP safe)

This version auto-detects your Render Pipeline and picks a compatible shader:
- Built-in RP → **Particles/Additive**
- URP → **Universal Render Pipeline/Particles/Unlit** (configured to *Additive*)
- Fallbacks: **URP/Unlit**, **HDRP/Unlit**, **Particles/Standard Unlit**

It also drops a copy of the prefab into `Resources/OutloopVFX/` so you can spawn it at runtime with:
```csharp
Outloop.VFX.HitFXPlayer.Spawn(position, normal);
```

## Setup
1. Unzip into your Unity project root (`Assets/` merges).
2. Unity menu: **Tools → Outloop → Create PingPong Hit FX Prefab**.
3. Prefab paths:
   - `Assets/Outloop/PingPongHitFX/Generated/Prefabs/PingPongHitFX.prefab`
   - `Assets/Outloop/PingPongHitFX/Generated/Resources/OutloopVFX/PingPongHitFX.prefab`

## Quick API
```csharp
// position = impact point, normal = hit normal
Outloop.VFX.HitFXPlayer.Spawn(position, normal);
Outloop.VFX.HitFXPlayer.Spawn(position, normal, new Color(0.9f, 1f, 1f)); // with tint
```
