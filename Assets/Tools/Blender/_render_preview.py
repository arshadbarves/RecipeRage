import bpy
import os

OUT = "/Users/arshadbarves/MyProject/Projects/RecipeRage/Assets/Tools/Blender/preview_tutorial.png"

scene = bpy.context.scene
# ensure preview camera is active
cam = bpy.data.objects.get("CAM_Preview")
if cam is not None:
    scene.camera = cam

scene.render.engine = 'BLENDER_EEVEE'
scene.render.resolution_x = 960
scene.render.resolution_y = 540
scene.render.resolution_percentage = 100
scene.render.image_settings.file_format = 'PNG'
scene.render.filepath = OUT
scene.render.film_transparent = False

# Render only the level collection + kit objects: hide chef/tomato collections
for cn in ("COL_Characters_Chef", "COL_Ingredients_Tomato"):
    c = bpy.data.collections.get(cn)
    if c is not None:
        c.hide_render = True

bpy.ops.render.render(write_still=True)

# unhide again
for cn in ("COL_Characters_Chef", "COL_Ingredients_Tomato"):
    c = bpy.data.collections.get(cn)
    if c is not None:
        c.hide_render = False

result = {"rendered": os.path.exists(OUT), "path": OUT}
