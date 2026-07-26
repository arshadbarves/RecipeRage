import bpy
import json

# Inspect current scene + ensure collection scaffolding exists.
def ensure_collection(name, parent=None):
    c = bpy.data.collections.get(name)
    if c is None:
        c = bpy.data.collections.new(name)
        (parent or bpy.context.scene.collection).children.link(c)
    return c

scene_col = bpy.context.scene.collection
cols = {}
for grp in ("Stations", "Characters", "Ingredients", "Props", "_Kit"):
    cols[grp] = ensure_collection("COL_" + grp)

info = {
    "objects": [o.name for o in bpy.context.scene.objects],
    "collections": [c.name for c in bpy.data.collections],
    "active_file": bpy.data.filepath,
}
result = info
