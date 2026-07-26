import bpy
import json

ROOT = "/Users/arshadbarves/MyProject/Projects/RecipeRage/Assets/Tools/Blender"

# ---- wipe scene ----
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
for block in (bpy.data.materials, bpy.data.curves, bpy.data.meshes, bpy.data.cameras, bpy.data.lights):
    pass  # keep materials; we rebuild below


def hex_to_rgba(h):
    h = h.lstrip('#')
    r, g, b = (int(h[i:i+2], 16) / 255.0 for i in (0, 2, 4))
    return (r, g, b, 1.0)


def make_mat(name, hex_color, rough=0.9, metallic=0.0):
    m = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    m.use_nodes = True
    bsdf = m.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = hex_to_rgba(hex_color)
    bsdf.inputs["Roughness"].default_value = rough
    bsdf.inputs["Metallic"].default_value = metallic
    if "Specular IOR Level" in bsdf.inputs:
        bsdf.inputs["Specular IOR Level"].default_value = 0.2
    return m


PALETTE = {
    # team
    "M_TeamA": "#E74C3C", "M_TeamB": "#3498DB",
    # environment
    "M_Wood": "#C89B6A", "M_WoodDark": "#A87F52", "M_Metal": "#9AA5B1",
    "M_MetalDark": "#6E7A86", "M_Counter": "#ECF0F1", "M_Plate": "#FDFDFB",
    "M_Floor": "#E8D9B8", "M_ChefSkin": "#F2C9A4", "M_ChefCoat": "#F7F7F5",
    "M_ChefPants": "#4A4E57",
    # ingredients
    "M_Tomato": "#E74C3C", "M_Onion": "#D9B8E6", "M_Garlic": "#F5F0E1",
    "M_Lettuce": "#7ECC6F", "M_Mushroom": "#C8A27A", "M_Chicken": "#F2C14E",
    "M_Beef": "#A94442", "M_Fish": "#5DADE2", "M_Rice": "#FBF7EC",
    "M_Pasta": "#F2D57E",
}

created = []
for name, hexv in PALETTE.items():
    make_mat(name, hexv)
    created.append(name)

# ---- fixed top-down 3/4 camera matching Unity Slice 1 ----
cam_data = bpy.data.cameras.new("CAM_Main")
cam = bpy.data.objects.new("CAM_Main", cam_data)
bpy.context.scene.collection.objects.link(cam)
cam.location = (0.0, -8.0, 12.0)   # Blender: -Y front, +Z up  == Unity (0,12,-8)
cam.rotation_euler = (1.0472, 0.0, 0.0)  # 60 deg about X
bpy.context.scene.camera = cam

# ---- key light ----
sun_data = bpy.data.lights.new("LGT_Sun", type='SUN')
sun_data.energy = 3.0
sun = bpy.data.objects.new("LGT_Sun", sun_data)
bpy.context.scene.collection.objects.link(sun)
sun.rotation_euler = (0.6, 0.2, 0.0)

# world ambient
world = bpy.data.worlds.get("World") or bpy.data.worlds.new("World")
bpy.context.scene.world = world
world.use_nodes = True
bg = world.node_tree.nodes.get("Background")
bg.inputs[0].default_value = (0.9, 0.9, 0.95, 1.0)
bg.inputs[1].default_value = 0.6

# save kit
bpy.ops.wm.save_as_mainfile(filepath=ROOT + "/_Kit.blend", check_existing=False)

result = {"kit_saved": True, "materials": created, "count": len(created)}
