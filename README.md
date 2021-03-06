## A short unofficial primer on how to write shaders for StereoKit!

_First, a word of caution. I'm not a rockstar coder, so whatever is written here including the code examples, comes without any guarantees. Proceed at own risk!_

### Some StereoKit syntax

StereoKit shaders are based on the HLSL language, you can read up on it more at [Microsofts excellent 
HLSL documentation](https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl).

The entry points for the vertex and fragment shader in StereoKit has to be specifically: 
 
    Vertex shader: vs  
    Fragment shader: ps

The way you set defaults for variables in the shader itself, also has a specific syntax:

    //--PointLightPosition = 1,1,1,0  
    //--LightRangeRcp = 1  
    //--LightColor = 1,1,1  
    //--DirToLight = 1,1,1,0  
    //--specExp = 1

### Anatomy of a simple ambient light shader in StereoKit

    #include "stereokit.hlsli"

A header we always need to include in StereoKit shaders.

    //--name = app/ambient

    //--tex_scale   = 1
    //--diffuse     = white
    //--AmbientUp	= 0,0,0
    //--AmbientDown = 0,0,0

Some default value declarations.

    float4 AmbientUp;
    float4 AmbientDown;
    float tex_scale;
    Texture2D diffuse : register(t0);
    SamplerState diffuse_s : register(s0);

Shader vectors from our C# program.

    struct vsIn
    {
	    float4 pos : SV_Position;
	    float3 norm : NORMAL0;
	    float2 uv : TEXCOORD0;
    };

Declaration of a struct for the values we want to deal with in the vertex shader.

    struct psIn
    {
	    float4 pos : SV_Position;
	    float2 uv : TEXCOORD0;
	    float3 norm : TEXCOORD1;
	    uint view_id : SV_RenderTargetArrayIndex;
    };

Declaration of a struct for the values we want to deal with in the fragment shader.

    psIn vs(vsIn input, uint id : SV_InstanceID)
    {
	    psIn o;
	    o.view_id = id % sk_view_count;
	    id = id / sk_view_count;

	    float4 world = mul(input.pos, sk_inst[id].world);
	    o.pos = mul(world, sk_viewproj[o.view_id]);

	    o.uv = input.uv * tex_scale;
	    o.norm = normalize(mul(input.norm, (float3x3) sk_inst[id].world));
	
	    return o;
    }

This is the vertex shader, here we set up our MVP matrix, UV coordinates and normals.

    // Ambient calculation helper function
    float3 CalcAmbient(float3 normal, float3 color)
    {
	    // Convert from [-1, 1] to [0, 1]
	    float up = normal.y * 0.5 + 0.5;

	    // Calculate the ambient value
	    float3 ambient = AmbientDown.rgb + up * AmbientUp.rgb;

	    // Apply the ambient value to the color
	    return ambient * color;
    }

A small function to calculate the ambient light.

    float4 ps(psIn input) : SV_TARGET
    {
	    // Sample the texture
	    float3 diffuseColor = diffuse.Sample(diffuse_s, input.uv).rgb;

	    // Calculate the ambient color
	    float3 AmbientColor = CalcAmbient(input.norm, diffuseColor);

	    // Return the ambient color
	    return float4(AmbientColor, 1.0);
    }

And finally, the fragment shader where RGB is calculated for each pixel on the mesh.

Now, to control the amount of Ambient light emitted from the top or bottom in the shader from our C# program, we utilize 
the shader vectors we set up earlier in the shader itself.

    float4 AmbientUp;
    float4 AmbientDown;

In the main loop we can then fire off values to these by using the .SetVector function:

    CustomMaterial.SetVector("AmbientUp", new Vec4(0.2, 0.2, 0.2, 0));

To check this out, build the example app in this repo and set the Light Type to Ambient.




## Here are some additional things that can be useful when working with shaders in StereoKit.
<details><summary>Resetting StereoKits default lighting to a pitch-black environment.</summary>
<p>

StereoKit ships with batteries included. By default, a Spherical Harmonics environment is created which
provides ambient lighting. You can play around with this in the [StereoKit SkyDemo](https://github.com/maluoi/StereoKit/blob/master/Examples/StereoKitTest/Demos/DemoSky.cs).

However, when working with shaders (especially light shaders!), it can sometimes be preferable
to work in a completely dark environment. That way you can be certain that only your own shader code
is affecting the materials you build.

First set the Spherical Harmonics light itself to emit nothing but black,  
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

Once that's done, build a new lighting solution, generate a CubeMap from it and ask the StereoKit
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

If you would try and fire this up, you would see nothing but black. StereoKit runs fine, but there's
nothing to see. This is because StereoKit applies the Spherical Harmonics lighting to all the default
"StereoKit" things, such as the hands, the UI windows etc.

Let's fix the hands first.  
The hands have a gradient material applied, let's start by overriding that with another base material:

    Default.MaterialHand.Shader = Shader.Unlit;

The above applies a built in StereoKit shader called Shader.Unlit to the hand. It's pretty much as the name implies, 
a shader that requires no lighting.
A fully lit hand however is not very aesthetically pleasing, so we can use a gradient to control the 
transparency, just like how the hand looks like in StereoKit with it's default lighting.
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

After that, it's a matter of just calling the function with our new gradient settings. For instance:

    ColorizeFingers(16,
            new Gradient(new GradientKey(new Color(0.75f, 0.75f, 0.75f, 0.75f), 1)),
            new Gradient(
                new GradientKey(new Color(.4f, .4f, .4f, 0), 0),
                new GradientKey(new Color(.6f, .6f, .6f, 0), 0.4f),
                new GradientKey(new Color(.8f, .8f, .8f, 1), 0.55f),
                new GradientKey(new Color(0.75f, 0.75f, 0.75f, 0.75f), 0.75f))); 

Ok, we move on to making the UI materials work in a pitch-black environment.

As before, set the Default UI material to the unlit shader.

    Default.MaterialUI.Shader = Shader.Unlit;

But that's not going to be enough, if you ran the code now with just the unlit shader applied, you would
see that only a few items such as slider knobs etc. were fully visible. The UI windows panel itself is still
black. This is because StereoKit uses a special shader for its UI panels, called
**shader_builtin_ui_quadrant**. 

Let's fish this shader out of the StereoKit source code and remove the default lighting. The shader is 
in [_StereoKit/StereoKitC/shaders_builtin/shader_builtin_ui_quadrant.hlsl_](https://github.com/maluoi/StereoKit/blob/master/StereoKitC/shaders_builtin/shader_builtin_ui_quadrant.hlsl).
Copy the shader file to your own directory and open it up. Find the line
_o.color.rgb *= Lighting(o.normal);_ and comment it out.  

Then set the default material_ui_quadrant  shader to this new tweaked version. I added _unlit_ to the name for clarity.

    Material.Find("default/material_ui_quadrant").Shader = Shader.FromFile("shader_builtin_ui_quadrant_unlit.hlsl");

The StereoKit hands and UI windows can now be seen, even in complete darkness.
	
</p>
</details>





