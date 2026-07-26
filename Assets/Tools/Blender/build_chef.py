import bpy
import os

ROOT = "/Users/arshadbarves/MyProject/Projects/RecipeRage/Assets/Tools/Blender"
ART = "/Users/arshadbarves/MyProject/Projects/RecipeRage/Assets/Art/Characters"

def mat(name):
    return bpy.data.materials.get(name)

def clear_scene():
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)

def box(name, loc, scale, material, bevel=0.0):
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=loc)
    o = bpy.context.active_object; o.name = name; o.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if material: o.data.materials.append(material)
    if bevel > 0.0:
        m = o.modifiers.new("Bevel", 'BEVEL'); m.width = bevel; m.segments = 2
    return o

def cyl(name, loc, radius, depth, material, vertices=14, rot=None):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth, location=loc)
    o = bpy.context.active_object; o.name = name
    if rot: o.rotation_euler = rot
    if material: o.data.materials.append(material)
    return o

def sphere(name, loc, radius, material, seg=16, rings=12, scale=(1,1,1)):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=seg, ring_count=rings, radius=radius, location=loc)
    o = bpy.context.active_object; o.name = name; o.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if material: o.data.materials.append(material)
    for p in o.data.polygons: p.use_smooth = True
    return o

clear_scene()

# proportions (total ~1.7m), origin at feet
# legs
cyl("legL", (-0.11,0,0.35), 0.09, 0.7, mat("M_ChefPants"), 12)
cyl("legR", (0.11,0,0.35), 0.09, 0.7, mat("M_ChefPants"), 12)
# shoes
sphere("shoeL", (-0.11,-0.04,0.06), 0.11, mat("M_MetalDark"), 12, 8, scale=(1,1.4,0.6))
sphere("shoeR", (0.11,-0.04,0.06), 0.11, mat("M_MetalDark"), 12, 8, scale=(1,1.4,0.6))
# torso (coat) - slightly tapered box
torso = box("torso", (0,0,0.95), (0.30,0.20,0.35), mat("M_ChefCoat"), 0.06)
# apron (team accent) - front panel
box("apron", (0,-0.20,0.95), (0.24,0.02,0.30), mat("M_TeamA"), 0.02)
# arms
cyl("armL", (-0.36,-0.02,1.05), 0.07, 0.55, mat("M_ChefCoat"), 12, rot=(0,0.35,0.2))
cyl("armR", (0.36,-0.02,1.05), 0.07, 0.55, mat("M_ChefCoat"), 12, rot=(0,-0.35,-0.2))
# hands
sphere("handL", (-0.46,-0.12,0.80), 0.08, mat("M_ChefSkin"), 12, 8)
sphere("handR", (0.46,-0.12,0.80), 0.08, mat("M_ChefSkin"), 12, 8)
# head
sphere("head", (0,0,1.45), 0.20, mat("M_ChefSkin"), 20, 16)
# chef hat: band (accent) + puffy top (white)
cyl("hatband", (0,0,1.62), 0.21, 0.10, mat("M_TeamA"), 18)
sphere("hattop", (0,0,1.76), 0.20, mat("M_ChefCoat"), 18, 12, scale=(1,1,0.7))

# join
parts = [o for o in bpy.context.scene.objects if o.type == 'MESH']
bpy.ops.object.select_all(action='DESELECT')
for o in parts: o.select_set(True)
bpy.context.view_layer.objects.active = parts[0]
bpy.ops.object.join()
sm = bpy.context.active_object
sm.name = "SK_Chef"
bpy.ops.object.origin_set(type='ORIGIN_GEOMETRY', center='BOUNDS')
minz = min((sm.matrix_world @ v.co).z for v in sm.data.vertices)
sm.location.z -= minz
sm.location.x = 0.0
sm.location.y = 0.0

bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT,"Characters","Chef.blend"), check_existing=False)
tris = sum(len(p.vertices)-2 for p in sm.data.polygons)
# height
zs = [(sm.matrix_world @ v.co).z for v in sm.data.vertices]
height = max(zs) - min(zs)
bpy.ops.export_scene.gltf(filepath=os.path.join(ART,"SK_Chef.glb"), export_format='GLB',
                          export_yup=True, export_apply=True,
                          export_cameras=False, export_lights=False)

result = {"asset": "SK_Chef", "tris": tris, "height_m": round(height,3)}
