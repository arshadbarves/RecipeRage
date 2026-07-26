import bpy
import os

ROOT = "/Users/arshadbarves/MyProject/Projects/RecipeRage/Assets/Tools/Blender"
ART = "/Users/arshadbarves/MyProject/Projects/RecipeRage/Assets/Art/Stations"

# ---------- helpers ----------
def mat(name):
    return bpy.data.materials.get(name)

def clear_scene():
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    for c in list(bpy.data.curves):
        bpy.data.curves.remove(c)

def box(name, loc, scale, material, bevel=0.0):
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=loc)
    o = bpy.context.active_object
    o.name = name
    o.scale = (scale[0], scale[1], scale[2])
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if material:
        o.data.materials.append(material)
    if bevel > 0.0:
        mod = o.modifiers.new("Bevel", 'BEVEL')
        mod.width = bevel
        mod.segments = 2
    return o

def cyl(name, loc, radius, depth, material, vertices=16):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth, location=loc)
    o = bpy.context.active_object
    o.name = name
    if material:
        o.data.materials.append(material)
    return o

def sphere(name, loc, radius, material, seg=16, rings=12, scale=(1,1,1)):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=seg, ring_count=rings, radius=radius, location=loc)
    o = bpy.context.active_object
    o.name = name
    o.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if material:
        o.data.materials.append(material)
    for p in o.data.polygons:
        p.use_smooth = True
    return o

def collect_parts():
    return [o for o in bpy.context.scene.objects if o.type == 'MESH']

def finish_asset(asset_name):
    """Join meshes, name SM_<asset>, save .blend, export .glb. Returns tri count."""
    parts = collect_parts()
    bpy.ops.object.select_all(action='DESELECT')
    for o in parts:
        o.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    bpy.ops.object.join()
    sm = bpy.context.active_object
    sm.name = "SM_" + asset_name
    # origin to base center
    bpy.ops.object.origin_set(type='ORIGIN_GEOMETRY', center='BOUNDS')
    # move so base sits at z=0
    minz = min((sm.matrix_world @ v.co).z for v in sm.data.vertices)
    sm.location.z -= minz
    sm.location.x = 0.0
    sm.location.y = 0.0
    blend_path = os.path.join(ROOT, "Stations", asset_name + ".blend")
    bpy.ops.wm.save_as_mainfile(filepath=blend_path, check_existing=False)
    tris = sum(len(p.vertices) - 2 for p in sm.data.polygons)
    glb_path = os.path.join(ART, "SM_" + asset_name + ".glb")
    bpy.ops.export_scene.gltf(filepath=glb_path, export_format='GLB',
                              export_yup=True, export_apply=True,
                              export_cameras=False, export_lights=False)
    return {"asset": asset_name, "tris": tris, "glb": glb_path}

results = []

# ---------- 1. IngredientCrate ----------
clear_scene()
box("body", (0,0,0.25), (0.55,0.45,0.05), mat("M_Wood"), 0.01)         # base
for sx in (-0.52, 0.52):
    box("side", (sx,0,0.45), (0.04,0.45,0.28), mat("M_WoodDark"), 0.005)
for sy in (-0.42, 0.42):
    box("end", (0,sy,0.45), (0.55,0.04,0.28), mat("M_WoodDark"), 0.005)
# slats
for i,z in enumerate((0.32, 0.45, 0.58)):
    box("slat_f", (0,-0.44,z), (0.56,0.02,0.04), mat("M_Wood"), 0.003)
    box("slat_b", (0,0.44,z), (0.56,0.02,0.04), mat("M_Wood"), 0.003)
# produce poking out
sphere("p1", (-0.2,0,0.62), 0.16, mat("M_Tomato"))
sphere("p2", (0.1,0.1,0.62), 0.15, mat("M_Lettuce"))
sphere("p3", (0.25,-0.1,0.60), 0.13, mat("M_Onion"))
# team-trim corner bands
for sx in (-0.52, 0.52):
    for sy in (-0.42, 0.42):
        box("trim", (sx,sy,0.62), (0.05,0.05,0.10), mat("M_TeamA"), 0.003)
results.append(finish_asset("IngredientCrate"))

# ---------- 2. CuttingBoard ----------
clear_scene()
box("counter", (0,0,0.5), (0.6,0.6,0.5), mat("M_Wood"), 0.02)          # base block
box("top", (0,0,1.0), (0.62,0.62,0.04), mat("M_Counter"), 0.01)         # counter top
box("board", (0,0.05,1.06), (0.34,0.26,0.03), mat("M_WoodDark"), 0.02)  # cutting board
# knife
box("blade", (0.25,-0.05,1.05), (0.02,0.16,0.005), mat("M_Metal"), 0.002)
cyl("handle", (0.25,-0.24,1.05), 0.02, 0.12, mat("M_WoodDark"), 10).rotation_euler=(1.5708,0,0)
# team trim front
box("trim", (0,-0.6,0.9), (0.62,0.02,0.08), mat("M_TeamA"), 0.003)
results.append(finish_asset("CuttingBoard"))

# ---------- 3. Stove ----------
clear_scene()
box("body", (0,0,0.5), (0.7,0.7,0.5), mat("M_Metal"), 0.03)             # body
box("cooktop", (0,0,1.0), (0.72,0.72,0.04), mat("M_MetalDark"), 0.01)   # top
for sx in (-0.2, 0.2):
    for sy in (-0.2, 0.2):
        cyl("burner", (sx,sy,1.05), 0.12, 0.03, mat("M_MetalDark"), 16)
# pot on one burner
cyl("pot", (-0.2,-0.2,1.18), 0.16, 0.22, mat("M_Metal"), 20)
cyl("potrim", (-0.2,-0.2,1.30), 0.18, 0.03, mat("M_MetalDark"), 20)
# control knobs
for i,sx in enumerate((-0.25, 0.0, 0.25)):
    cyl("knob", (sx,-0.71,0.85), 0.04, 0.03, mat("M_MetalDark"), 12).rotation_euler=(1.5708,0,0)
# team-trim front panel
box("trim", (0,-0.7,0.4), (0.6,0.02,0.25), mat("M_TeamA"), 0.005)
results.append(finish_asset("Stove"))

# ---------- 4. PlateStation ----------
clear_scene()
box("cabinet", (0,0,0.5), (0.5,0.5,0.5), mat("M_Wood"), 0.02)
box("top", (0,0,1.0), (0.52,0.52,0.04), mat("M_Counter"), 0.01)
# stack of plates
for i in range(4):
    cyl("plate", (0,0,1.05+i*0.03), 0.20, 0.025, mat("M_Plate"), 20)
box("trim", (0,-0.5,0.9), (0.52,0.02,0.06), mat("M_TeamA"), 0.003)
results.append(finish_asset("PlateStation"))

# ---------- 5. Counter ----------
clear_scene()
box("base", (0,0,0.5), (0.8,0.6,0.5), mat("M_Wood"), 0.02)
box("top", (0,0,1.0), (0.82,0.62,0.04), mat("M_Counter"), 0.01)
box("trim", (0,-0.6,0.9), (0.8,0.02,0.06), mat("M_TeamA"), 0.003)
results.append(finish_asset("Counter"))

# ---------- 6. ServingCounter ----------
clear_scene()
box("base", (0,0,0.5), (1.1,0.6,0.5), mat("M_Wood"), 0.02)
box("top", (0,0,1.0), (1.12,0.62,0.04), mat("M_Counter"), 0.01)
# pass window trim (two posts + lintel)
for sx in (-0.5, 0.5):
    box("post", (sx,0,1.4), (0.05,0.05,0.4), mat("M_WoodDark"), 0.005)
box("lintel", (0,0,1.75), (0.55,0.05,0.05), mat("M_WoodDark"), 0.005)
# service bell
sphere("bell", (0.3,0,1.08), 0.06, mat("M_Metal"), 16, 12, scale=(1,1,0.7))
cyl("bellbase", (0.3,0,1.03), 0.07, 0.02, mat("M_MetalDark"), 16)
# team-trim front
box("trim", (0,-0.6,0.55), (1.1,0.02,0.15), mat("M_TeamA"), 0.005)
results.append(finish_asset("ServingCounter"))

result = {"stations": results}
