## A short unofficial primer on how to write shaders for StereoKit!

_First, a word of caution. I'm not a rockstar coder, so whatever is written here inculding the code examples, comes without any guarantees. Proceed at own risk!_

### Some StereoKit syntax & pecularities

The entry points for the pixel and fragment shader in StereoKit has to be specifically: 
 
    Pixel shader: vs  
    Fragment shader: ps


The way you set defaults for variables in the shader itself, also has a specific syntax:

    //--PointLightPosition = 1,1,1,0  
    //--LightRangeRcp = 1  
    //--LightColor = 1,1,1  
    //--DirToLight = 1,1,1,0  
    //--specExp = 1


StereoKit ships with batteries included. By default, a Spherical Harmonics environment is created which
provides ambient the ligting. You can play around with this in the [StereoKit SkyDemo](https://github.com/maluoi/StereoKit/blob/master/Examples/StereoKitTest/Demos/DemoSky.cs).

However, when working with shaders (especially light shaders!), it can sometimes be preferable
to work in a completely dark environment. That way you know you can be certain that only your own shader code
is affecting the materials you build.

So let's do that next!

### Resetting StereoKit default lighting to a pitch black environment.

First we set the Spherical Harmonics light itself to emit nothing but black  
by adding a light to the top and bottom, both having their RGB set to zero.

    lights.Add(new Light  
    {  
        pose = new Pose(Vec3.Up * 25 * U.cm, Quat.LookDir(-Vec3.Forward)),  
        color = Vec3.Zero  
    });  
    lights.Add(new Light  
    {  
        pose = new Pose(-Vec3.Up * 25 * U.cm, Quat.LookDir(-Vec3.Forward)),  
        color = Vec3.Zero  
    });

Once that's done, we build a new lighting solution, generate a CubeMap from it and ask the StereoKit
renderer to use that from now on.

    SphericalHarmonics lighting = SphericalHarmonics.FromLights(lights
    .ConvertAll(a => new SHLight
    {
        directionTo = a.pose.position.Normalized,
        color = Color.HSV(a.color) * 0.5f
    }).ToArray());

    Renderer.SkyTex = Tex.GenCubemap(lighting);
    Renderer.SkyLight = lighting;

The complete code for this is [here](https://github.com/ClonedPuppy/SK_Shader_info/blob/master/Light.cs)

So if you would try and fire this up, you would see nothing but black. StereoKit runs fine, but there's
nothing to see. This is because StereoKit applies the Spherical Harmonics lighting to all the default
"StereoKit" things, such as the hands, the UI windows etc.

So let's fix this. We do the hands first.  
The hands have a gradient material applied, let's start by overriding that with our own base material:

    Default.MaterialHand.Shader = Shader.Unlit;

The above applies a built in StereoKit shader called Shader.Unlit to the hand. It's pretty much as the name implies, 
a shader that requires no lighting.
A fully lit hand however is not very aestheticly pleasing, so we can use a gradient to control the 
transperancy, just like how the hand looks like in StereoKit with it's default lighting.
You can use a function from StereoKit source code which steps through all the fingers and sets the gradient values.

    private static void ColorizeFingers(int size, Gradient horizontal, Gradient vertical)
    {
        Tex tex = new Tex(TexType.Image, TexFormat.Rgba32Linear)
        {
            AddressMode = TexAddress.Clamp
        };

        Color32[] pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            Color v = vertical.Get(1 - y / (size - 1.0f));
            for (int x = 0; x < size; x++)
            {
                Color h = horizontal.Get(x / (size - 1.0f));
                pixels[x + y * size] = v * h;
            }
        }
        tex.SetColors(size, size, pixels);

        Default.MaterialHand[MatParamName.DiffuseTex] = tex;
    }

After that, we just need to call the function with our new gradient settings. For instance:

    ColorizeFingers(16,
            new Gradient(new GradientKey(new Color(0.75f, 0.75f, 0.75f, 0.75f), 1)),
            new Gradient(
                new GradientKey(new Color(.4f, .4f, .4f, 0), 0),
                new GradientKey(new Color(.6f, .6f, .6f, 0), 0.4f),
                new GradientKey(new Color(.8f, .8f, .8f, 1), 0.55f),
                new GradientKey(new Color(0.75f, 0.75f, 0.75f, 0.75f), 0.75f))); 

Ok, hand is done! Now we move on to making the UI materials work in a pitch dark environment.

As before, we set the Default UI material to the unlit shader.

    Default.MaterialUI.Shader = Shader.Unlit;

But that's not going to be enough, if you ran the code now with just the unlit shader applied, you would
see that only a few items such as slider knobs etc were fully visible. The UI windows panel itself is still
black. This is because StereoKit uses a special shader for it's UI panels, called
**shader_builtin_ui_quadrant_unlit**. 

So let's fish this shader out of the StereoKit source code and remove the default lighting. The shader is 
in [_StereoKit/StereoKitC/shaders_builtin/shader_builtin_ui_quadrant.hlsl_](https://github.com/maluoi/StereoKit/blob/master/StereoKitC/shaders_builtin/shader_builtin_ui_quadrant.hlsl).
Copy the shader file to your own directory, and open it up. Find the line
_o.color.rgb *= Lighting(o.normal);_ and comment it out.  

Then set the default material_ui_quadrant  shader to this new tweaked version.

    Material.Find("default/material_ui_quadrant").Shader = Shader.FromFile("shader_builtin_ui_quadrant_unlit.hlsl");

Done! The StereoKit hands and UI windows can now be seen, even in pitch darkness.












