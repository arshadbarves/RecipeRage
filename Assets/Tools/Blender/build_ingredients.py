import bpy
import os

ROOT = "/Users/arshadbarves/MyProject/Projects/RecipeRage/Assets/Tools/Blender"
ART = "/Users/arshadbarves/MyProject/Projects/RecipeRage/Assets/Art/Ingredients"

def mat(name):
    return bpy.data.materials.get(name)

def hex_to_rgba(h):
    h = h.lstrip('#')
    return tuple(int(h[i:i+2],16)/255.0 for i in (0,2,4)) + (1.0,)

def lerp_hex(a, b, t):
    a = a.lstrip('#'); b = b.lstrip('#')
    out = []
    for i in (0,2,4):
        x = int(a[i:i+2],16); y = int(b[i:i+2],16)
        out.append(int(round(x + (y-x)*t)))
    return "#{:02X}{:02X}{:02X}".format(*out)

def make_variant_mat(name, hex_color):
    m = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    m.use_nodes = True
    bsdf = m.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = hex_to_rgba(hex_color)
    bsdf.inputs["Roughness"].default_value = 0.9
    bsdf.inputs["Metallic"].default_value = 0.0
    return m

def clear_scene():
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)

def sphere(name, loc, radius, material, seg=12, rings=8, scale=(1,1,1)):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=seg, ring_count=rings, radius=radius, location=loc)
    o = bpy.context.active_object; o.name = name; o.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if material: o.data.materials.append(material)
    for p in o.data.polygons: p.use_smooth = True
    return o

def box(name, loc, scale, material, bevel=0.0):
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=loc)
    o = bpy.context.active_object; o.name = name; o.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if material: o.data.materials.append(material)
    if bevel>0:
        m=o.modifiers.new("Bevel",'BEVEL'); m.width=bevel; m.segments=2
    return o

def cyl(name, loc, radius, depth, material, vertices=12):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth, location=loc)
    o=bpy.context.active_object; o.name=name
    if material: o.data.materials.append(material)
    return o

def finish(asset_name, export=True):
    parts=[o for o in bpy.context.scene.objects if o.type=='MESH']
    bpy.ops.object.select_all(action='DESELECT')
    for o in parts: o.select_set(True)
    bpy.context.view_layer.objects.active=parts[0]
    bpy.ops.object.join()
    sm=bpy.context.active_object; sm.name="SM_"+asset_name
    bpy.ops.object.origin_set(type='ORIGIN_GEOMETRY', center='BOUNDS')
    minz=min((sm.matrix_world@v.co).z for v in sm.data.vertices)
    sm.location.z-=minz; sm.location.x=0.0; sm.location.y=0.0
    tris=sum(len(p.vertices)-2 for p in sm.data.polygons)
    glb=os.path.join(ART,"SM_"+asset_name+".glb")
    bpy.ops.export_scene.gltf(filepath=glb, export_format='GLB', export_yup=True,
                              export_apply=True, export_cameras=False, export_lights=False)
    return {"asset":asset_name,"tris":tris}

# ingredient builders return base hex and build raw mesh at origin-ish
def build_raw(name, m):
    if name=="Tomato":
        sphere("t",(0,0,0.15),0.16,m,scale=(1,1,0.9))
        cyl("stem",(0,0,0.30),0.02,0.06,mat("M_Lettuce"),8)
    elif name=="Onion":
        sphere("o",(0,0,0.15),0.16,m,scale=(1,1,1.05))
        cyl("tip",(0,0,0.32),0.02,0.05,m,8)
    elif name=="Garlic":
        sphere("g",(0,0,0.12),0.13,m,scale=(1,1,1.1))
        cyl("tip",(0,0,0.26),0.02,0.05,m,8)
    elif name=="Lettuce":
        sphere("l",(0,0,0.15),0.17,m,scale=(1,1,0.85))
        sphere("l2",(0.08,0,0.13),0.12,m)
    elif name=="Mushroom":
        cyl("stem",(0,0,0.10),0.06,0.16,mat("M_Garlic"),10)
        sphere("cap",(0,0,0.22),0.15,m,scale=(1,1,0.6))
    elif name=="Chicken":
        sphere("c",(0,0,0.13),0.16,m,scale=(1.1,0.9,0.8))
        cyl("bone",(0.18,0,0.10),0.03,0.12,mat("M_Garlic"),8).rotation_euler=(0,1.2,0)
    elif name=="Beef":
        box("b",(0,0,0.10),(0.18,0.12,0.09),m,0.03)
        box("fat",(0,-0.11,0.10),(0.16,0.02,0.07),mat("M_Garlic"),0.01)
    elif name=="Fish":
        sphere("f",(0,0,0.12),0.16,m,scale=(1.4,0.7,0.5))
        # tail
        box("tail",(0.22,0,0.12),(0.08,0.02,0.08),m,0.01)
    elif name=="Rice":
        cyl("r",(0,0,0.06),0.14,0.12,m,16)
        sphere("rt",(0,0,0.14),0.13,m,scale=(1,1,0.5))
    elif name=="Pasta":
        cyl("p",(0,0,0.05),0.15,0.10,m,16)
        for i in range(3):
            sphere("n"+str(i),( -0.06+i*0.06,0.02,0.12),0.05,m,10,6)

BASE_HEX = {
 "Tomato":"#E74C3C","Onion":"#D9B8E6","Garlic":"#F5F0E1","Lettuce":"#7ECC6F",
 "Mushroom":"#C8A27A","Chicken":"#F2C14E","Beef":"#A94442","Fish":"#5DADE2",
 "Rice":"#FBF7EC","Pasta":"#F2D57E"}

results=[]
for name, basehex in BASE_HEX.items():
    basemat = mat("M_"+name)
    # RAW
    clear_scene(); build_raw(name, basemat)
    results.append(finish(name))
    # CHOPPED: 3 small wedges/discs using base mat
    clear_scene()
    for i in range(3):
        sphere("c"+str(i),(-0.10+i*0.10,0,0.05),0.07,basemat,10,6,scale=(1,1,0.6))
    results.append(finish(name+"_Chopped"))
    # COOKED/BURNT: raw silhouette but burnt variant material
    burnt_hex = lerp_hex(basehex, "#2B2B2B", 0.55)
    bmat = make_variant_mat("MI_"+name+"_Burnt", burnt_hex)
    clear_scene(); build_raw(name, bmat)
    results.append(finish(name+"_Burnt"))

# save the working blend (last state) - full regeneration source is this script
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT,"Ingredients","Ingredients.blend"), check_existing=False)
result={"count":len(results),"items":results}
