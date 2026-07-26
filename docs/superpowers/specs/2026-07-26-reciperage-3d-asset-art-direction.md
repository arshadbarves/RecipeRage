# RecipeRage — 3D Asset Art Direction & Generation Spec

**Date:** 2026-07-26
**Status:** Draft (addendum to `2026-07-25-reciperage-rebuild-design.md`)
**Authors:** AI Assistant + Project Owner

---

## 1. Purpose & Spec Update

This addendum defines the 3D asset pipeline for RecipeRage and **supersedes one line** of the main design spec.

> **Spec update (approved 2026-07-26):** The main spec's Executive Summary describes RecipeRage as a "top-down **2D** multiplayer cooking competition game". This is updated to: RecipeRage is a **top-down 3D** multiplayer cooking competition game. The fixed top-down 3/4 camera (`position (0, 12, -8)`, `rotation (60, 0, 0)` — Slice 1, Task 7) is retained; only the rendering dimensionality changes. Overcooked-style presentation is achieved with real 3D models rather than sprites.
>
> All other visual pillars are unchanged: bright colorful flat palette, **no gradients**, soft shadows, mobile-first landscape.

3D assets are **generated procedurally in Blender** (via the Blender MCP bridge) and exported to Unity's URP project. This document is the source of truth for what gets generated, how it looks, and where it lands.

---

## 2. Toolchain

| Component | Value |
|---|---|
| Blender | 5.2.0 LTS (`/Applications/Blender.app`) |
| Bridge | Blender Lab MCP add-on, socket `127.0.0.1:9876` |
| MCP server | `blender-mcp` via `uv --directory /Users/arshadbarves/blender_mcp/mcp run blender-mcp` |
| Unity render pipeline | URP 17.3 (`com.unity.render-pipelines.universal`) |
| Export format | glTF Binary (`.glb`), Y-up, 1 unit = 1 m |

> **Security note (from Blender):** the add-on executes LLM-generated Python with no sandbox. Generation code only builds scene geometry/materials and saves/exports to `Assets/Art/` and `Assets/Tools/Blender/`. It never touches other files.

---

## 3. Art Direction

- **Style:** stylized low-poly, clean silhouettes, slightly chunky proportions (Overcooked / Brawl Stars read).
- **Color:** flat solid colors, high saturation, **no gradients** and no texture maps. Color lives on materials / vertex color.
- **Shading:** Principled BSDF — `roughness 0.9`, `metallic 0.0`, `specular 0.2`. Flat faces (no smooth shading except where a soft curve reads better, e.g. chef hat, tomato).
- **Lighting/readability:** assets are modeled to read from the fixed top-down 3/4 camera; strong top faces, darker sides via a single key light + ambient. Soft shadows only.
- **Team identity:** **outfit-accent tint** — chef body stays neutral; only hat band / apron / trim are tinted per team. Station trim (edges, handles, flags) is tintable the same way. Implemented as a dedicated `MI_TeamA` / `MI_TeamB` material slot so Unity can swap team color cheaply and skins stay compatible.

---

## 4. Palette

### Team
| Slot | Hex | Use |
|---|---|---|
| Team A | `#E74C3C` | chef hat band/apron/trim, station trim |
| Team B | `#3498DB` | chef hat band/apron/trim, station trim |

### Environment / shared
| Slot | Hex | Use |
|---|---|---|
| Wood | `#C89B6A` | crates, cutting board, counters |
| Wood dark | `#A87F52` | trim, legs, edges |
| Metal | `#9AA5B1` | stove body, knives, pots |
| Metal dark | `#6E7A86` | stove grates, burners |
| Counter | `#ECF0F1` | prep/serving counter tops |
| Plate | `#FDFDFB` | plate ceramic |
| Floor (neutral) | `#E8D9B8` | tutorial / neutral kitchen tile |
| Chef skin | `#F2C9A4` | neutral body |
| Chef coat | `#F7F7F5` | chef jacket |
| Chef pants | `#4A4E57` | neutral pants |

### Ingredients
| Ingredient | Base | Cooked / Burnt |
|---|---|---|
| Tomato | `#E74C3C` | `#C0392B` / `#5B2C24` |
| Onion | `#D9B8E6` | `#B795CF` / `#4E3A55` |
| Garlic | `#F5F0E1` | `#E4D8B8` / `#6B6150` |
| Lettuce | `#7ECC6F` | `#5EAE54` / `#3A4A33` |
| Mushroom | `#C8A27A` | `#A67F57` / `#4F4034` |
| Chicken | `#F2C14E` | `#D9A03A` / `#6E4F1F` |
| Beef | `#A94442` | `#8A3634` / `#3E2322` |
| Fish | `#5DADE2` | `#4A8FC0` / `#2C3E50` |
| Rice | `#FBF7EC` | `#EFE6CF` / `#6E6656` |
| Pasta | `#F2D57E` | `#E3C25E` / `#6E6136` |

Burnt tint is applied by lerping base toward near-black at the material level (a `Burnt` material variant), not by re-modeling.

---

## 5. Poly Budgets (mobile)

| Asset | Target tris |
|---|---|
| Chef | 1,500–3,000 |
| Station | 300–800 |
| Ingredient | 50–200 |
| Plate | 100–200 |
| Floor/wall kit piece | 50–300 |

---

## 6. Naming, Scale, Export

- **Mesh names:** `SM_<Name>` for static meshes (e.g. `SM_Stove`), `SK_Chef` for the riggable chef.
- **Materials:** `M_<Name>` base, `MI_<Name>` variants (e.g. `MI_TeamA`, `MI_TeamB`, `MI_Tomato_Burnt`).
- **Scale:** 1 Blender unit = 1 m. Chef ≈ 1.7 m tall. Station counters ≈ 1 m work height. Ingredients ≈ 0.15–0.3 m.
- **Origin:** at the base center of every asset (so stations snap to floor, chef pivots at feet).
- **Forward:** assets face +Y in Blender (glTF exporter converts to Unity −Z forward).
- **Export:** one `.glb` per asset via `export_scene.gltf` (`export_yup=True`, `export_apply=True`, materials on, no cameras/lights in the file).
- **Source `.blend`:** each asset also saved under `Assets/Tools/Blender/<Group>/<Name>.blend` so it is regenerable and tweakable.

### Output layout (Unity project)
```
Assets/
├── Tools/Blender/            # generation .blend sources + shared kit (not imported by Unity build)
│   ├── _Kit.blend            # shared palette materials, camera, light, export helpers
│   ├── Stations/  Characters/  Ingredients/  Props/
└── Art/
    ├── Characters/           # SK_Chef.glb (+ menu showcase chefs in a later batch)
    ├── Stations/             # SM_*.glb
    ├── Ingredients/          # SM_<Ingredient>[_Chopped|_Burnt].glb
    ├── Props/                # SM_Plate.glb, floor/wall kit
    └── Maps/                 # themed map shells (later batch)
```

---

## 7. Batch Plan

### Batch 1 — Core gameplay (this spec, generated now)
Needed for Slice 1 to render real gameplay.

| Group | Assets |
|---|---|
| Stations (6) | `SM_IngredientCrate`, `SM_CuttingBoard`, `SM_Stove`, `SM_PlateStation`, `SM_Counter`, `SM_ServingCounter` |
| Chef (1) | `SK_Chef` base + `MI_TeamA`/`MI_TeamB` accent slots |
| Ingredients (10 × 3 states) | Tomato, Onion, Garlic, Lettuce, Mushroom, Chicken, Beef, Fish, Rice, Pasta — raw / chopped / cooked-burnt |
| Plate (1) | `SM_Plate` (holds up to 4 ingredients) |
| Kitchen kit | tileable floor tile + counter-edge wall pieces |

### Batch 2 — Map shells (Slice 5)
Beach BBQ, Forest Campfire, Pirate Ship, Tutorial ground shells + 4–6 themed props each (palm, grill, campfire, mast, etc.).

### Batch 3 — Menu showcase chefs (Slice 5)
Higher-detail Gordon, Julia, Marco, Gustavo for Main Menu / Lobby / Chef Select (RenderTexture showcase).

---

## 8. Per-Asset Modeling Notes (Batch 1)

- **IngredientCrate** — open wooden crate, slatted sides, a few generic produce shapes poking out; team-trim corner bands.
- **CuttingBoard** — thick board on a low counter block + a knife prop (blade `M_Metal`, handle `M_WoodDark`).
- **Stove** — block body (`M_Metal`), 2 burner grates (`M_MetalDark`), a pot; a flat area on top to show cook progress; team-trim front panel.
- **PlateStation** — low cabinet with a stack of empty plates on top.
- **Counter/Prep** — plain counter block (`M_Counter` top, `M_Wood` base), 2-item temporary storage.
- **ServingCounter** — wider counter with a bell + pass window trim; team-trim.
- **Chef** — neutral body: head, chef hat (band = accent), coat (`M_ChefCoat`), apron (accent), pants, shoes. Simple, rig-ready proportions (separate head/torso/limbs). Holds pose for carry.
- **Ingredients** — chunky single-piece primitives with flat colors; chopped = 2–3 wedge/disc pieces; burnt = same mesh, `MI_*_Burnt` material.
- **Plate** — shallow rimmed disc (`M_Plate`).
- **Floor/wall kit** — 1×1 m floor tile + 1 m counter-edge wall segment, neutral colors, re-skinnable per map.

---

## 9. Acceptance Criteria

- All Batch 1 `.glb` files exist under `Assets/Art/…` and import in Unity at correct scale (chef ≈ 1.7 m) with Y-up orientation.
- Materials are flat-color, roughness ~0.9, no textures, no gradients.
- Each asset has a regenerable `.blend` under `Assets/Tools/Blender/…`.
- Team accent is isolated to a swappable material slot.
- Poly counts within budgets in §5.
