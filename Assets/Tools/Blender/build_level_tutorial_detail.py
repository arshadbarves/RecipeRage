import bpy
import os
import math

ROOT = "/Users/arshadbarves/MyProject/Projects/RecipeRage/Assets/Tools/Blender"
COLNAME = "COL_Level_Tutorial"

def mat(name):
    return bpy.data.materials.get(name)

def hex_mat(name, hex_color, rough=0.9):
    m = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    m.use_nodes = True
    b = m.node_tree.nodes.get("Principled BSDF")
    h = hex_color.lstrip('#')
    b.inputs["Base Color"].default_value = tuple(int(h[i:i+2],16)/255.0 for i in (0,2,4)) + (1.0,)
    b.inputs["Roughness"].default_value = rough
    b.inputs["Metallic"].default_value = 0.0
    return m

# extra materials for dressing
hex_mat("M_FloorAccent", "#D9C49A")   # darker tile
hex_mat("M_WallUpper", "#F3EDE0")     # warm light wall
hex_mat("M_Green", "#6FBF73")         # plant green
hex_mat("M_Terracotta", "#C96F4A")    # pot
hex_mat("M_Chalk", "#3E4A3A")         # chalkboard
hex_mat("M_Frame", "#8A6B45")         # chalkboard frame
hex_mat("M_Rug", "#D98E73")           # floor mat
hex_mat("M_Brass", "#C9A24B")

def link_only(col, o):
    for c in list(o.users_collection):
        c.objects.unlink(o)
    col.objects.link(o)

def box(col, name, loc, scale, material, bevel=0.0, rot=None):
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=loc)
    o=bpy.context.active_object; o.name=name; o.scale=scale
    if rot: o.rotation_euler=rot
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if material: o.data.materials.append(material)
    if bevel>0:
        m=o.modifiers.new("Bevel",'BEVEL'); m.width=bevel; m.segments=2
    link_only(col,o); return o

def cyl(col, name, loc, radius, depth, material, vertices=16, rot=None):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth, location=loc)
    o=bpy.context.active_object; o.name=name
    if rot: o.rotation_euler=rot
    if material: o.data.materials.append(material)
    link_only(col,o); return o

def sphere(col, name, loc, radius, material, seg=14, rings=10, scale=(1,1,1)):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=seg, ring_count=rings, radius=radius, location=loc)
    o=bpy.context.active_object; o.name=name; o.scale=scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if material: o.data.materials.append(material)
    for p in o.data.polygons: p.use_smooth=True
    link_only(col,o); return o

def cone(col, name, loc, r1, r2, depth, material, vertices=12):
    bpy.ops.mesh.primitive_cone_add(vertices=vertices, radius1=r1, radius2=r2, depth=depth, location=loc)
    o=bpy.context.active_object; o.name=name
    if material: o.data.materials.append(material)
    link_only(col,o); return o

col = bpy.data.collections.get(COLNAME)

# ============ 1. FLOOR TILES (checkerboard) ============
# replace plain floor with tiled look: keep base, add accent tiles grid
tile = 1.0
for ix in range(-7,7):
    for iy in range(-4,5):
        if (ix+iy) % 2 == 0:
            box(col, "Tile", (ix*tile*0.5+0.25, iy*tile*0.5, 0.001), (0.48,0.48,0.005), mat("M_FloorAccent"), 0.0)

# floor mats in front of each station
for x in (-5.0,-2.5,0.0,2.5,5.0):
    box(col, "Mat", (x,2.3,0.02), (0.7,0.5,0.02), mat("M_Rug"), 0.03)

# ============ 2. WALL DRESSING ============
# wainscoting: wood lower half on back wall
box(col, "Wainscot", (0,4.25,0.55), (7.0,0.06,0.55), mat("M_Wood"), 0.01)
# upper wall lighter panel
box(col, "WallUpperPanel", (0,4.28,1.6), (7.0,0.05,0.6), mat("M_WallUpper"), 0.0)
# cornice
box(col, "Cornice", (0,4.2,1.95), (7.0,0.08,0.06), mat("M_WoodDark"), 0.01)

# window on back wall (frame + panes) at center-right gap above stove/serving
wx = 3.75
box(col, "WinFrame", (wx,4.30,1.55), (0.7,0.06,0.55), mat("M_Frame"), 0.02)
box(col, "WinGlass", (wx,4.33,1.55), (0.58,0.02,0.44), mat("M_WallUpper"), 0.0)
box(col, "WinMullV", (wx,4.34,1.55), (0.02,0.03,0.44), mat("M_Frame"), 0.0)
box(col, "WinMullH", (wx,4.34,1.55), (0.58,0.03,0.02), mat("M_Frame"), 0.0)

# chalkboard menu on back wall left-center
bx = -3.75
box(col, "ChalkFrame", (bx,4.30,1.55), (0.8,0.05,0.55), mat("M_Frame"), 0.02)
box(col, "ChalkBoard", (bx,4.33,1.55), (0.7,0.02,0.45), mat("M_Chalk"), 0.0)
# chalk lines (menu scribbles)
for i in range(3):
    box(col, "ChalkLine", (bx,4.35,1.72-i*0.15), (0.5-0.1*i,0.01,0.015), mat("M_Plate"), 0.0)

# wall shelf with jars (right of chalkboard, above plate station)
box(col, "Shelf", (0.0,4.18,1.5), (0.9,0.18,0.03), mat("M_WoodDark"), 0.01)
for i,sx in enumerate((-0.5,-0.15,0.2,0.55)):
    cyl(col, "Jar", (sx,4.18,1.62), 0.07, 0.18, mat("M_Terracotta"), 12)
    cyl(col, "JarLid", (sx,4.18,1.72), 0.075, 0.03, mat("M_WoodDark"), 12)

# hanging utensils rail (left wall near cutting board)
cyl(col, "Rail", (-2.5,4.05,1.7), 0.015, 1.2, mat("M_Brass"), 10, rot=(0,math.pi/2,0))
for i,sx in enumerate((-2.9,-2.5,-2.1)):
    cyl(col, "Hook", (sx,4.05,1.62), 0.01, 0.1, mat("M_Brass"), 8)
    # ladle/spatula heads
    sphere(col, "Utensil", (sx,4.05,1.5), 0.05, mat("M_Metal"), 10, 8, scale=(1,0.4,1.3))

# ============ 3. STATION DETAIL UPGRADES ============
# stove: add pot lid + handle + a kettle on second burner
stove_x, stove_y = 2.5, 3.4
cyl(col, "PotLid", (stove_x-0.2,stove_y-0.2,1.32), 0.15, 0.02, mat("M_MetalDark"), 20)
sphere(col, "PotLidKnob", (stove_x-0.2,stove_y-0.2,1.35), 0.03, mat("M_Brass"), 10, 8)
# pot handles
for sx in (-0.36,-0.04):
    cyl(col, "PotHandle", (stove_x+sx,stove_y-0.2,1.25), 0.015, 0.1, mat("M_MetalDark"), 8, rot=(0,math.pi/2,0))
# kettle on right burner
sphere(col, "Kettle", (stove_x+0.2,stove_y+0.2,1.16), 0.13, mat("M_Brass"), 14, 10, scale=(1,1,0.9))
cone(col, "KettleSpout", (stove_x+0.32,stove_y+0.2,1.2), 0.03, 0.01, 0.12, mat("M_Brass"), 8)

# cutting board: chopped veg bits
cut_x, cut_y = -2.5, 3.4
for i in range(3):
    sphere(col, "ChopBit", (cut_x-0.08+i*0.08, cut_y+0.05, 1.10), 0.035, mat("M_Tomato"), 8, 6, scale=(1,1,0.6))

# serving counter: ticket rail with tickets
sv_x, sv_y = 5.0, 3.4
box(col, "TicketRail", (sv_x,sv_y-0.1,1.6), (0.45,0.02,0.03), mat("M_Brass"), 0.005)
for i,dx in enumerate((-0.25,0.0,0.25)):
    box(col, "Ticket", (sv_x+dx,sv_y-0.1,1.5), (0.08,0.005,0.1), mat("M_Plate"), 0.0)

# crate: add one more produce
sphere(col, "Crate_p3", (-5.0+0.25,3.4-0.1,0.60), 0.13, mat("M_Onion"))

# ============ 4. PLANTS ============
def plant(x, y):
    cyl(col, "Pot", (x,y,0.18), 0.16, 0.32, mat("M_Terracotta"), 14)
    cyl(col, "PotRim", (x,y,0.36), 0.18, 0.06, mat("M_Terracotta"), 14)
    for i,ang in enumerate((0,2.1,4.2)):
        lx = x + 0.1*math.cos(ang); ly = y + 0.1*math.sin(ang)
        sphere(col, "Leaf", (lx,ly,0.6+0.08*i), 0.16, mat("M_Green"), 10, 8, scale=(0.6,0.6,1.4))

plant(-6.3, 3.9)   # back-left corner
plant(6.3, 3.9)    # back-right corner

# ============ 5. WARM LIGHTING ============
sun = bpy.data.objects.get("LGT_Key")
if sun and sun.type=='LIGHT':
    sun.data.energy = 4.0
    sun.data.color = (1.0, 0.956, 0.84)  # warm sunlight
world = bpy.context.scene.world
if world and world.use_nodes:
    bg = world.node_tree.nodes["Background"]
    bg.inputs[0].default_value = (0.98, 0.94, 0.88, 1.0)  # warm ambient
    bg.inputs[1].default_value = 0.8

bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT,"RecipeRage.blend"), check_existing=False)

tris=0; meshes=0
for o in col.objects:
    if o.type=='MESH':
        meshes+=1
        tris+=sum(len(p.vertices)-2 for p in o.data.polygons)
result={"collection":COLNAME,"mesh_parts":meshes,"tris":tris}
