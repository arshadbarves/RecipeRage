import bpy
import os

ROOT = "/Users/arshadbarves/MyProject/Projects/RecipeRage/Assets/Tools/Blender"
ART = "/Users/arshadbarves/MyProject/Projects/RecipeRage/Assets/Art/Maps"
os.makedirs(ART, exist_ok=True)
COLNAME = "COL_Level_Tutorial"

def mat(name, fallback=(0.8,0.8,0.8,1)):
    m = bpy.data.materials.get(name)
    return m

def new_collection(name):
    c = bpy.data.collections.get(name) or bpy.data.collections.new(name)
    if c.name not in bpy.context.scene.collection.children:
        bpy.context.scene.collection.children.link(c)
    return c

def link_only(col, o):
    for c in list(o.users_collection):
        c.objects.unlink(o)
    col.objects.link(o)

def box(col, name, loc, scale, material, bevel=0.0):
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=loc)
    o = bpy.context.active_object; o.name = name; o.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if material: o.data.materials.append(material)
    if bevel > 0:
        m = o.modifiers.new("Bevel", 'BEVEL'); m.width = bevel; m.segments = 2
    link_only(col, o)
    return o

def cyl(col, name, loc, radius, depth, material, vertices=16, rot=None):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth, location=loc)
    o = bpy.context.active_object; o.name = name
    if rot: o.rotation_euler = rot
    if material: o.data.materials.append(material)
    link_only(col, o)
    return o

def sphere(col, name, loc, radius, material, seg=14, rings=10, scale=(1,1,1)):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=seg, ring_count=rings, radius=radius, location=loc)
    o = bpy.context.active_object; o.name = name; o.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if material: o.data.materials.append(material)
    for p in o.data.polygons: p.use_smooth = True
    link_only(col, o)
    return o

col = new_collection(COLNAME)

# ---------- LEVEL SHELL ----------
# Floor: 14 x 9 m, tutorial-neutral tile
box(col, "Floor", (0,0,-0.1), (7.0,4.5,0.1), mat("M_Floor"), 0.0)
# Perimeter walls (low, so top-down reads interior) - back + two sides, open front
box(col, "Wall_Back", (0,4.4,1.0), (7.0,0.2,1.0), mat("M_Counter"), 0.02)
box(col, "Wall_Left", (-6.9,0,1.0), (0.2,4.5,1.0), mat("M_Counter"), 0.02)
box(col, "Wall_Right", (6.9,0,1.0), (0.2,4.5,1.0), mat("M_Counter"), 0.02)
# Wall base trim
box(col, "Trim_Back", (0,4.2,0.15), (7.0,0.05,0.15), mat("M_WoodDark"), 0.0)

# ---------- STATIONS (spec order L->R teaching flow) ----------
# Ingredient Crate
def crate(x, y):
    box(col, "Crate_body", (x,y,0.25), (0.55,0.45,0.05), mat("M_Wood"), 0.01)
    for sx in (-0.52,0.52):
        box(col, "Crate_side", (x+sx,y,0.45), (0.04,0.45,0.28), mat("M_WoodDark"), 0.005)
    for sy in (-0.42,0.42):
        box(col, "Crate_end", (x,y+sy,0.45), (0.55,0.04,0.28), mat("M_WoodDark"), 0.005)
    sphere(col, "Crate_p1", (x-0.2,y,0.62), 0.16, mat("M_Tomato"))
    sphere(col, "Crate_p2", (x+0.1,y+0.1,0.62), 0.15, mat("M_Lettuce"))

def cutting(x, y):
    box(col, "Cut_counter", (x,y,0.5), (0.6,0.6,0.5), mat("M_Wood"), 0.02)
    box(col, "Cut_top", (x,y,1.0), (0.62,0.62,0.04), mat("M_Counter"), 0.01)
    box(col, "Cut_board", (x,y+0.05,1.06), (0.34,0.26,0.03), mat("M_WoodDark"), 0.02)
    box(col, "Cut_blade", (x+0.25,y-0.05,1.05), (0.02,0.16,0.005), mat("M_Metal"), 0.002)

def plate_station(x, y):
    box(col, "Plate_cab", (x,y,0.5), (0.5,0.5,0.5), mat("M_Wood"), 0.02)
    box(col, "Plate_top", (x,y,1.0), (0.52,0.52,0.04), mat("M_Counter"), 0.01)
    for i in range(4):
        cyl(col, "Plate_%d"%i, (x,y,1.05+i*0.03), 0.20, 0.025, mat("M_Plate"), 20)

def stove(x, y):
    box(col, "Stove_body", (x,y,0.5), (0.7,0.7,0.5), mat("M_Metal"), 0.03)
    box(col, "Stove_top", (x,y,1.0), (0.72,0.72,0.04), mat("M_MetalDark"), 0.01)
    for sx in (-0.2,0.2):
        for sy in (-0.2,0.2):
            cyl(col, "Stove_burner", (x+sx,y+sy,1.05), 0.12, 0.03, mat("M_MetalDark"), 16)
    cyl(col, "Stove_pot", (x-0.2,y-0.2,1.18), 0.16, 0.22, mat("M_Metal"), 20)

def serving(x, y):
    box(col, "Serve_base", (x,y,0.5), (1.1,0.6,0.5), mat("M_Wood"), 0.02)
    box(col, "Serve_top", (x,y,1.0), (1.12,0.62,0.04), mat("M_Counter"), 0.01)
    for sx in (-0.5,0.5):
        box(col, "Serve_post", (x+sx,y,1.4), (0.05,0.05,0.4), mat("M_WoodDark"), 0.005)
    box(col, "Serve_lintel", (x,y,1.75), (0.55,0.05,0.05), mat("M_WoodDark"), 0.005)
    sphere(col, "Serve_bell", (x+0.3,y,1.08), 0.06, mat("M_Metal"), 16, 12, scale=(1,1,0.7))

# Layout: stations along the back wall, player space in front (y negative toward camera)
crate(-5.0, 3.4)          # Ingredient Crate (left)
cutting(-2.5, 3.4)        # Cutting Board
plate_station(0.0, 3.4)   # Plate Station (center)
stove(2.5, 3.4)           # Stove
serving(5.0, 3.4)         # Serving Counter (right)

# ---------- LIGHTING + CAMERA for preview ----------
# remove existing preview cam/light if any
for n in ("CAM_Preview","LGT_Key"):
    old = bpy.data.objects.get(n)
    if old: bpy.data.objects.remove(old, do_unlink=True)

cam_data = bpy.data.cameras.new("CAM_Preview")
cam = bpy.data.objects.new("CAM_Preview", cam_data)
bpy.context.scene.collection.objects.link(cam)
cam.location = (0.0, -10.0, 12.0)
cam.rotation_euler = (1.0472, 0.0, 0.0)  # 60 deg
cam_data.lens = 35
bpy.context.scene.camera = cam

sun_data = bpy.data.lights.new("LGT_Key", type='SUN')
sun_data.energy = 3.5
sun = bpy.data.objects.new("LGT_Key", sun_data)
bpy.context.scene.collection.objects.link(sun)
sun.rotation_euler = (0.7, 0.15, 0.0)

world = bpy.data.worlds.get("World") or bpy.data.worlds.new("World")
bpy.context.scene.world = world
world.use_nodes = True
world.node_tree.nodes["Background"].inputs[0].default_value = (0.92,0.93,0.97,1.0)
world.node_tree.nodes["Background"].inputs[1].default_value = 0.7

# save master + a standalone level blend
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT,"RecipeRage.blend"), check_existing=False)

# count tris in this collection only
tris = 0
meshes = 0
for o in col.objects:
    if o.type == 'MESH':
        meshes += 1
        tris += sum(len(p.vertices)-2 for p in o.data.polygons)

result = {"collection": COLNAME, "mesh_parts": meshes, "tris": tris, "stations": 5}
