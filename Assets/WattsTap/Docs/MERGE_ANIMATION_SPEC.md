# Merge Animation — UI Animation Specification for Unity

## Overview

This document describes the **Item Merge Animation** — a UI animation sequence that plays when two identical items are combined (merged) into a higher-level item in a mobile game. The animation is designed for a sci-fi themed merge/crafting interface.

**Source:** 66 frames (frame_0025 through frame_0090), ~24.5 fps, total duration ~2.7 seconds.
**Resolution:** 362×648 px (portrait mobile).

---

## Scene Layout (Static Elements)

### Background
- Dark teal/green sci-fi themed background with halftone dot pattern in the lower portion
- Metallic panels and circuit-like patterns in the upper area
- A horizontal divider separates the upper merge zone from the lower stats zone

### Merge Machine (Central Element)
- A **large octagonal/hexagonal metallic frame** centered horizontally in the upper half of the screen
- Dark grey metallic border with beveled edges (sci-fi console aesthetic)
- Inside: a large **bright green square slot** — this is where the main item is displayed
- Two small **dark metallic side tabs** on left and right sides of the machine (decorative pistons/clamps)

### Merge Button
- A small **pink/red rectangular button** at the bottom-right of the merge machine area
- Appears to be the "confirm merge" trigger

---

## Animation Sequence — Phase Breakdown

### PHASE 1: Idle / Pre-Merge State (frames 25–30, ~0.2s)
**Duration:** ~5 frames  
**Description:** The initial state showing three items ready for merging.

**Layout:**
- **Main slot (center, large):** One item card (brown boots) inside the merge machine's green slot. The card has a bright green background, the item icon in the center, and a small green rarity badge in the upper-left corner
- **Two smaller item cards** positioned below the machine, slightly to the left and right:
  - Left card: same boots item, smaller scale (~40% of main card)
  - Right card: same boots item, smaller scale (~40% of main card)
- Both small cards have the same green card background and rarity badges

**Visual state:** Everything is static. Items are at rest positions.

---

### PHASE 2: Merge Pull-In / Absorption (frames 30–43, ~0.5s)
**Duration:** ~13 frames  
**Easing:** EaseInBack or EaseInQuad (accelerating movement)

**Description:** The two small item cards begin sliding/pulling toward the center merge machine slot.

**Animation details:**
1. **Frames 30–35:** Both small cards start moving — they slide slightly inward and begin **rotating** (small tilt ~5-10°). Left card rotates CW, right card rotates CCW. Both cards also start scaling down slightly.
2. **Frames 35–40:** The cards continue accelerating toward the center. They are now noticeably closer to the main slot. Rotation increases. Cards are shrinking.
3. **Frames 40–43:** The cards converge rapidly into the main slot. By frame 43, all three items are overlapping inside the main card area — the two small cards are almost fully absorbed into the center position.
4. **Frame 44 (key frame):** All three item icons are visible inside the main slot, compressed together. The small cards have fully merged into the slot boundary.

**Motion path:** Both small cards follow a **curved arc** upward and inward toward the center of the main slot — not a straight line.

**Key properties animated:**
- Position: (start: below machine, left/right) → (end: center of main slot)
- Scale: (start: ~0.4) → (end: ~0.3 then 0)
- Rotation: (start: 0°) → (end: ±15° then reset)
- Alpha: stays 1.0 throughout

---

### PHASE 3: Flash / Transformation (frames 44–47, ~0.12s)
**Duration:** ~3 frames  
**Description:** An instant bright flash occurs as the items fuse.

**Animation details:**
1. **Frame 44→45 (instant transition):** 
   - All item icons disappear
   - The main slot fills with a **bright white/cyan glow** — full white-out of the slot area
   - The green background of the main card is replaced with a white/light cyan fill
   - Text **"Success!"** appears at the top of the screen in **bold white font with a slight red/warm tint** (off-white to pinkish-white)
   - Item name **"Highboots"** appears below the merge machine in **cyan/light blue text**
   - A **cyan/blue upward-pointing triangle/arrow** VFX appears at the top edge of the machine frame

2. **Frames 45–47:**
   - The white glow in the slot begins to clear slightly
   - A small white item silhouette begins forming in the center of the glow (teardrop/boot shape)
   - **Cyan diamond-shaped light bursts** emanate from the corners of the machine frame
   - The machine frame itself gains a subtle **white/cyan outline glow**

**Key VFX:**
- Additive white flash overlay on the slot
- Cyan triangular "energy burst" above the machine
- Outer glow on the machine frame border

---

### PHASE 4: New Item Reveal with Scale Bounce (frames 47–55, ~0.33s)
**Duration:** ~8 frames  
**Easing:** EaseOutBack (overshoot bounce)

**Description:** The new upgraded item card materializes with a dramatic scale-up and light ray effect.

**Animation details:**
1. **Frames 47–48:** The new item card appears at small scale (~0.3) inside the slot. It's a **blue card** (upgraded from green!) with:
   - Blue background
   - The same boots item icon but now with **"Lv. 1"** text badge
   - A small blue rarity/type badge in the upper-left
   - White/cyan glow still present around it

2. **Frames 48–52 (Scale overshoot):** 
   - The new card **rapidly scales up** past its final size to ~1.2x with EaseOutBack
   - Simultaneously, the entire **merge machine scales up** to ~1.15x
   - **Light ray VFX:** Large cyan/white triangular light rays emanate diagonally from behind the machine — like a starburst pattern (2-3 large triangular beams going upper-left, upper-right, and downward)
   - The machine frame has a strong **white outer glow/bloom**

3. **Frames 52–55 (Settle back):**
   - The card and machine scale back down to 1.0x (elastic settle)
   - The light ray VFX fades out over these frames
   - The glow reduces to a subtle residual bloom
   - Machine returns to its normal appearance

**Key properties animated:**
- Card scale: 0 → 0.3 → 1.2 → 1.0 (EaseOutBack)
- Machine scale: 1.0 → 1.15 → 1.0 (EaseOutBack)
- Light rays alpha: 0 → 1.0 → 0 (fade in fast, fade out over ~8 frames)
- Glow intensity: high → low

---

### PHASE 5: Stats Panel Slide-In (frames 57–75, ~0.75s)
**Duration:** ~18 frames  
**Easing:** EaseOutQuad or EaseOutCubic (decelerating)

**Description:** A stats comparison panel slides up from below the divider, revealing the item's improved stats one by one with staggered timing.

**Layout of stats panel (dark background area, lower half of screen):**

The stats appear **sequentially from top to bottom** with a stagger delay of ~5 frames each:

1. **"Max Level"** line (appears first, ~frame 58):
   - Label: "Max Level" — bold white text, centered
   - Values: **"20"** (white, left) → **green arrow icon** → **"30"** (green, right)

2. **"HP"** line (appears second, ~frame 63):
   - Label: "HP" — bold white text, centered
   - Values: **"53"** (white, left) → **green arrow icon** → **"75"** (green, right)

3. **"Unlock Skill"** line (appears third, ~frame 73):
   - Label: "Unlock Skill" — bold white text, centered
   - Value: **"HP +10%"** — green text below the label

**Animation for each stat line:**
- Slides in from below (Y offset ~30px) while fading in (alpha 0 → 1)
- Duration per line: ~5 frames
- Easing: EaseOutQuad

**Stat value format:**
- Old value in white → green right-arrow (▶) → New value in bright green
- This "old → new" pattern uses a green arrow/chevron icon between values

---

### PHASE 6: Final Hold / Idle (frames 75–90, ~0.6s)
**Duration:** ~15 frames  
**Description:** Everything is in its final position. The screen holds for the player to read the results.

**Final state:**
- "Success!" text remains at top
- Merge machine shows the new blue-tier item card with "Lv. 1"
- "Highboots" name below the machine
- All three stat lines fully visible in the lower panel
- A subtle diagonal **shine/glint** sweeps across the item card icon (white diagonal line moving left-to-right across the card, like a reflective gleam)

---

## Timing Summary Table

| Phase | Frames | Duration (s) | Description |
|-------|--------|--------------|-------------|
| 1 | 25–30 | 0.20 | Idle — 3 items displayed |
| 2 | 30–43 | 0.53 | Pull-in — small cards fly to center |
| 3 | 44–47 | 0.12 | Flash — white/cyan burst |
| 4 | 47–55 | 0.33 | Reveal — new item scales in with light rays |
| 5 | 57–75 | 0.75 | Stats — lines slide in sequentially |
| 6 | 75–90 | 0.61 | Hold — final state with shine effect |
| **Total** | **25–90** | **~2.7** | |

---

## Unity Implementation Notes

### Recommended Approach
Use **DOTween** (or Unity's built-in Animation/Animator) for all tweening. The entire sequence can be orchestrated with a **DOTween Sequence**.

### UI Hierarchy (Canvas)

```
MergeAnimationRoot (Canvas / CanvasGroup)
├── Background (Image — static sci-fi BG)
├── MergeZone
│   ├── MergeFrame (Image — octagonal metallic frame)
│   │   ├── MainSlot (Image — green/blue card background)
│   │   │   ├── ItemIcon (Image — boots sprite)
│   │   │   ├── LevelBadge (Text — "Lv. 1", hidden initially)
│   │   │   └── RarityBadge (Image — small corner icon)
│   │   ├── FlashOverlay (Image — white, alpha=0, Additive blend)
│   │   ├── GlowBorder (Image — cyan glow around frame, alpha=0)
│   │   └── ShineEffect (Image — diagonal white gradient, masked)
│   ├── LeftSideTab (Image — decorative)
│   ├── RightSideTab (Image — decorative)
│   ├── SmallCardLeft (same structure as MainSlot, smaller)
│   ├── SmallCardRight (same structure as MainSlot, smaller)
│   ├── LightRays (Image — cyan triangular rays, alpha=0, behind MergeFrame)
│   ├── TopTriangleVFX (Image — cyan upward triangle, alpha=0)
│   └── ItemNameLabel (Text — "Highboots", hidden initially)
├── SuccessText (Text — "Success!", hidden initially)
├── MergeButton (Button — pink/red)
└── StatsPanel (VerticalLayoutGroup, below divider)
    ├── StatLine_MaxLevel (CanvasGroup, alpha=0)
    │   ├── Label (Text — "Max Level")
    │   ├── OldValue (Text — "20")
    │   ├── ArrowIcon (Image — green arrow)
    │   └── NewValue (Text — "30", green)
    ├── StatLine_HP (CanvasGroup, alpha=0)
    │   ├── Label (Text — "HP")
    │   ├── OldValue (Text — "53")
    │   ├── ArrowIcon (Image — green arrow)
    │   └── NewValue (Text — "75", green)
    └── StatLine_UnlockSkill (CanvasGroup, alpha=0)
        ├── Label (Text — "Unlock Skill")
        └── Value (Text — "HP +10%", green)
```

### DOTween Sequence Pseudocode

```csharp
Sequence mergeSequence = DOTween.Sequence();

// PHASE 2: Pull-in (0.0s – 0.5s)
mergeSequence.Append(
    smallCardLeft.transform.DOMove(mainSlotCenter, 0.5f).SetEase(Ease.InBack)
);
mergeSequence.Join(
    smallCardLeft.transform.DOScale(0f, 0.5f).SetEase(Ease.InQuad)
);
mergeSequence.Join(
    smallCardLeft.transform.DORotate(new Vector3(0, 0, 15f), 0.5f, RotateMode.FastBeyond360)
);
mergeSequence.Join(
    smallCardRight.transform.DOMove(mainSlotCenter, 0.5f).SetEase(Ease.InBack)
);
mergeSequence.Join(
    smallCardRight.transform.DOScale(0f, 0.5f).SetEase(Ease.InQuad)
);
mergeSequence.Join(
    smallCardRight.transform.DORotate(new Vector3(0, 0, -15f), 0.5f, RotateMode.FastBeyond360)
);

// PHASE 3: Flash (0.5s – 0.62s)
mergeSequence.AppendCallback(() => {
    smallCardLeft.SetActive(false);
    smallCardRight.SetActive(false);
    SwapCardToUpgraded(); // change green card to blue card
    successText.SetActive(true);
    itemNameLabel.SetActive(true);
});
mergeSequence.Append(
    flashOverlay.DOFade(1f, 0.06f).SetEase(Ease.OutQuad)
);
mergeSequence.Join(
    glowBorder.DOFade(0.8f, 0.06f)
);
mergeSequence.Join(
    topTriangleVFX.DOFade(1f, 0.06f)
);

// PHASE 4: Reveal (0.62s – 0.95s)
mergeSequence.Append(
    flashOverlay.DOFade(0f, 0.15f)
);
mergeSequence.Join(
    newItemCard.transform.DOScale(1f, 0.33f).From(0f).SetEase(Ease.OutBack)
);
mergeSequence.Join(
    mergeFrame.transform.DOScale(1.15f, 0.15f).SetEase(Ease.OutQuad)
        .OnComplete(() => mergeFrame.transform.DOScale(1f, 0.18f).SetEase(Ease.InOutQuad))
);
mergeSequence.Join(
    lightRays.DOFade(1f, 0.08f).OnComplete(() => lightRays.DOFade(0f, 0.25f))
);
mergeSequence.Join(
    glowBorder.DOFade(0f, 0.3f).SetDelay(0.1f)
);
mergeSequence.Join(
    topTriangleVFX.DOFade(0f, 0.2f).SetDelay(0.1f)
);

// PHASE 5: Stats Slide-In (0.95s – 1.7s)
float statsDelay = 0.2f; // stagger between each line
mergeSequence.Append(
    statLineMaxLevel.DOFade(1f, 0.2f).SetEase(Ease.OutQuad)
);
mergeSequence.Join(
    statLineMaxLevel.transform.DOLocalMoveY(targetY_maxLevel, 0.2f)
        .From(targetY_maxLevel - 30f).SetEase(Ease.OutQuad)
);

mergeSequence.Append(
    statLineHP.DOFade(1f, 0.2f).SetEase(Ease.OutQuad).SetDelay(statsDelay)
);
mergeSequence.Join(
    statLineHP.transform.DOLocalMoveY(targetY_hp, 0.2f)
        .From(targetY_hp - 30f).SetEase(Ease.OutQuad)
);

mergeSequence.Append(
    statLineUnlockSkill.DOFade(1f, 0.2f).SetEase(Ease.OutQuad).SetDelay(statsDelay)
);
mergeSequence.Join(
    statLineUnlockSkill.transform.DOLocalMoveY(targetY_skill, 0.2f)
        .From(targetY_skill - 30f).SetEase(Ease.OutQuad)
);

// PHASE 6: Shine sweep
mergeSequence.Append(
    shineEffect.transform.DOLocalMoveX(shineEndX, 0.4f)
        .From(shineStartX).SetEase(Ease.InOutQuad).SetDelay(0.3f)
);
```

### Color Palette

| Element | Color (Hex) | Usage |
|---------|-------------|-------|
| Card BG (before merge) | `#7ED321` | Bright green, common rarity |
| Card BG (after merge) | `#4A90D9` | Blue, upgraded rarity |
| Machine frame | `#4A4A4A` | Dark grey metallic |
| Flash / Glow | `#FFFFFF` to `#00E5FF` | White with cyan tint |
| Light rays | `#00E5FF` | Cyan, additive blend |
| "Success!" text | `#FF6B6B` | Red/pink-white bold |
| Item name text | `#00BFFF` | Cyan |
| Stat old value | `#FFFFFF` | White |
| Stat new value | `#4CD964` | Bright green |
| Stat arrow | `#4CD964` | Bright green |
| "Unlock Skill" value | `#4CD964` | Bright green |
| Background | `#0D3B2E` | Dark teal |
| Halftone dots | `#1A5C45` | Slightly lighter teal |

### Visual Effects (VFX) Details

1. **Flash Overlay:** A white `Image` component filling the main slot, using additive blending (or just high alpha white). Animates alpha 0→1→0 quickly.

2. **Light Rays:** A pre-made sprite with 2-3 triangular beams radiating from center. Place behind the merge frame, set to additive blend mode. Scale up and fade in/out.

3. **Top Triangle VFX:** A small upward-pointing cyan triangle/arrow above the machine top edge. Fades in with the flash, fades out during the reveal.

4. **Glow Border:** A slightly larger version of the merge frame with blurred/soft edges and cyan color, additive blending. Creates the "energy border" effect.

5. **Shine Sweep:** A narrow diagonal white gradient strip (masked to the card area) that translates horizontally across the item icon. Creates a reflective gleam effect on the final item.

### Font Specifications

- **"Success!"** — Bold, rounded sans-serif (similar to Nunito Bold or Fredoka One), ~36pt, with subtle dark drop shadow
- **Item name ("Highboots")** — Medium weight, same font family, ~18pt, cyan colored
- **Stat labels ("Max Level", "HP", "Unlock Skill")** — Bold, ~16pt, white
- **Stat values** — Bold, ~20pt, white (old) and green (new)

### Sprite Assets Needed

1. `merge_frame` — Octagonal metallic frame (can be 9-sliced)
2. `card_bg_green` — Green rarity card background
3. `card_bg_blue` — Blue rarity card background  
4. `item_boots` — Boots item icon
5. `rarity_badge_green` — Small green corner badge
6. `rarity_badge_blue` — Small blue corner badge
7. `flash_white` — Simple white square (for overlay)
8. `glow_border` — Soft-edged version of merge frame
9. `light_rays` — Triangular starburst rays sprite
10. `triangle_vfx` — Small upward cyan triangle
11. `shine_streak` — Thin diagonal white gradient
12. `green_arrow` — Small green right-pointing arrow (▶)
13. `merge_button` — Pink/red button sprite
14. `side_tab` — Dark metallic side clamp decorations
15. `halftone_pattern` — Tileable halftone dot pattern for BG
