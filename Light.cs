using StereoKit;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SK_Shader_info
{
    class Light
    {
        [StructLayout(LayoutKind.Sequential)]
        struct BufferData01
        {
            public Vec4 lightColorR;
            public Vec4 lightColorG;
            public Vec4 lightColorB;
            public Vec4 lightPosX;
            public Vec4 lightPosY;
            public Vec4 lightPosZ;
            public Vec4 lightDirX;
            public Vec4 lightDirY;
            public Vec4 lightDirZ;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct BufferData02
        {
            public Vec4 ambientUp;
            public Vec4 ambientDown;
            public Vec4 lightRange;
            public Vec4 spotCosOuterCone;
            public Vec4 spotCosInnerConeRcp;
            public Vec4 capsuleLightLen;
        }

        static MaterialBuffer<BufferData01> matBuffer01;
        static MaterialBuffer<BufferData02> matBuffer02;

        static BufferData01 data01;
        static BufferData02 data02;

        static Light()
        {
            data01 = new BufferData01
            {
                lightColorR = new Vec4(0.5f, 0.5f, 0.5f, 0.5f),
                lightColorG = new Vec4(0.5f, 0.5f, 0.5f, 0.5f),
                lightColorB = new Vec4(0.5f, 0.5f, 0.5f, 0.5f),
                lightPosX = new Vec4(0.0738f, 0.05f, 0.08f, 0.1f),
                lightPosY = new Vec4(0.008f, 0.17f, 0.17f, 0.17f),
                lightPosZ = new Vec4(-0.182f, 0.62f, 0.62f, 0.62f),
                lightDirX = new Vec4(0f, 0f, 0f, 0f),
                lightDirY = new Vec4(0.70710677f, 0f, 0f, 0f),
                lightDirZ = new Vec4(0.7071067f, 1f, 1f, 1f),
            };
            matBuffer01 = new MaterialBuffer<BufferData01>(3);


            data02 = new BufferData02
            {
                ambientUp = new Vec4(0f, 0f, 0f, 0f),
                ambientDown = new Vec4(0f, 0f, 0f, 0f),
                lightRange = new Vec4(0f, 0f, 0f, 0f),
                spotCosOuterCone = new Vec4(0.9f, 0.9f, 0.9f, 0.9f),
                spotCosInnerConeRcp = new Vec4(60f, 60f, 60f, 60f),
                capsuleLightLen = new Vec4(0f, 0f, 0f, 0f),
            };
            matBuffer02 = new MaterialBuffer<BufferData02>(4);
        }


        static List<Light> lights = new List<Light>();
        Pose pose;
        Vec3 color;

        public static void Initialize()
        {
            lights.Add(new Light
            {
                pose = new Pose(Vec3.Up * 25 * U.cm, Quat.LookDir(-Vec3.Forward)),
                color = Vec3.Zero
                //color = V.XYZ(0.53f, 1f, 1f)
            });
            lights.Add(new Light
            {
                pose = new Pose(-Vec3.Up * 25 * U.cm, Quat.LookDir(-Vec3.Forward)),
                color = Vec3.Zero
                //color = V.XYZ(0.37f, 1f, 1f)
            });

            SphericalHarmonics lighting = SphericalHarmonics.FromLights(lights
            .ConvertAll(a => new SHLight
            {
                directionTo = a.pose.position.Normalized,
                color = Color.HSV(a.color) * 0.5f
            }).ToArray());

            Renderer.SkyTex = Tex.GenCubemap(lighting);
            Renderer.SkyLight = lighting;
            //Renderer.EnableSky = false;

            matBuffer01.Set(data01);
            matBuffer02.Set(data02);
        }
    }
}
