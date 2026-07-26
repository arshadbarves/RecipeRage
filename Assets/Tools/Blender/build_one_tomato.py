import bpy
import os

ROOT = "/Users/arshadbarves/MyProject/Projects/RecipeRage/Assets/Tools/Blender"
ART = "/Users/arshadbarves/MyProject/Projects/RecipeRage/Assets/Art/Ingredients"
ASSET = "Tomato"
GROUP = "Ingredients"

def mat(name):
    return bpy.data.materials.get(name)

def new_collection(name):
    c = bpy.data.collections.get(name) or bpy.data.collections.new(name)
    if c.name not in bpy.context.scene.collection.children:
        bpy.context.scene.collection.children.link(c)
    return c

def link_only(col, o):
    for c in list(o.users_collection):
        c.objects.unlink(o)
    col.objects.link(o)

def sphere(col, name, loc, radius, material, seg=12, rings=8, scale=(1,1,1)):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=seg, ring_count=rings, radius=radius, location=loc)
    o = bpy.context.active_object; o.name = name; o.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if material: o.data.materials.append(material)
    for p in o.data.polygons: p.use_smooth = True
    link_only(col, o)
    return o

def cyl(col, name, loc, radius, depth, material, vertices=10):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth, location=loc)
    o = bpy.context.active_object; o.name = name
    if material: o.data.materials.append(material)
    link_only(col, o)
    return o

col = new_collection("COL_{}_{}".format(GROUP, ASSET))

# build raw tomato
sphere(col, ASSET+"_body", (0,0,0.15), 0.16, mat("M_Tomato"), scale=(1,1,0.9))
cyl(col, ASSET+"_stem", (0,0,0.30), 0.02, 0.06, mat("M_Lettuce"), 8)

# join within this collection only
parts = [o for o in col.objects if o.type == 'MESH']
bpy.ops.object.select_all(action='DESELECT')
for o in parts: o.select_set(True)
bpy.context.view_layer.objects.active = parts[0]
bpy.ops.object.join()
sm = bpy.context.active_object
sm.name = "SM_" + ASSET
link_only(col, sm)
bpy.ops.object.origin_set(type='ORIGIN_GEOMETRY', center='BOUNDS')
minz = min((sm.matrix_world @ v.co).z for v in sm.data.vertices)
sm.location.z -= minz; sm.location.x = 0.0; sm.location.y = 0.0

# select only this asset and export selected
bpy.ops.object.select_all(action='DESELECT')
sm.select_set(True)
bpy.context.view_layer.objects.active = sm
glb = os.path.join(ART, "SM_"+ASSET+".glb")
bpy.ops.export_scene.gltf(filepath=glb, export_format='GLB', export_yup=True,
                          export_apply=True, use_selection=True,
                          export_cameras=False, export_lights=False)

tris = sum(len(p.vertices)-2 for p in sm.data.polygons)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT, "RecipeRage.blend"), check_existing=False)
result = {"asset": ASSET, "tris": tris, "collection": col.name, "glb": glb}
