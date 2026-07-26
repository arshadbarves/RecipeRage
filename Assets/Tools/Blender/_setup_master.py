import bpy
import os

ROOT = "/Users/arshadbarves/MyProject/Projects/RecipeRage/Assets/Tools/Blender"
MASTER = os.path.join(ROOT, "RecipeRage.blend")

# Move SK_Chef into its own collection inside the current file, then save as master.
def ensure_collection(name):
    c = bpy.data.collections.get(name) or bpy.data.collections.new(name)
    if c.name not in bpy.context.scene.collection.children:
        try:
            bpy.context.scene.collection.children.link(c)
        except RuntimeError:
            pass
    return c

chef_col = ensure_collection("COL_Characters_Chef")

chef = bpy.data.objects.get("SK_Chef")
moved = False
if chef is not None:
    # unlink from all current collections, link into its own
    for c in list(chef.users_collection):
        c.objects.unlink(chef)
    chef_col.objects.link(chef)
    moved = True

# Remove the placeholder group collections created earlier if empty.
for name in ("COL_Stations","COL_Characters","COL_Ingredients","COL_Props","COL__Kit"):
    c = bpy.data.collections.get(name)
    if c is not None and len(c.objects) == 0 and len(c.children) == 0:
        bpy.data.collections.remove(c)

bpy.ops.wm.save_as_mainfile(filepath=MASTER, check_existing=False)

result = {
    "master_saved": MASTER,
    "chef_in_collection": moved,
    "chef_collections": [c.name for c in chef.users_collection] if chef else [],
    "collections": [c.name for c in bpy.data.collections],
}
