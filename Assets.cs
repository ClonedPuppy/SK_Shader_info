using StereoKit;

namespace SK_Shader_info

{
    public static class Assets
    {
        public static Model floorMesh;
        public static Model bunny;
        public static Model dirLightModel;
        public static Model pointLightModel;
        public static Model spotLightModel;
        public static Model capsuleLightModel;

        static Material floorMat;
        public static Material bunnyMat;

        static Tex carpet;

        public static void Load()
        {
            // Textures
            carpet = Tex.FromFile("carpet.jpg");

            // Materials
            floorMat = new Material(Shader.FromFile("floor.hlsl"));
            bunnyMat = new Material(Shader.FromFile("spot.hlsl"));  // Set which light shader you want to use here!

            // Models
            floorMesh = Model.FromMesh(Mesh.GeneratePlane(new Vec2(40, 40), Vec3.Up, Vec3.Forward), floorMat);
            bunny = Model.FromFile("bunny.glb");
            dirLightModel = Model.FromFile("dirLight.glb", Shader.FromFile("LightModelShader.hlsl"));
            pointLightModel = Model.FromFile("pointLight.glb", Shader.FromFile("LightModelShader.hlsl"));
            spotLightModel = Model.FromFile("spotLight.glb", Shader.FromFile("LightModelShader.hlsl"));
            capsuleLightModel = Model.FromFile("capsuleLight.glb", Shader.FromFile("LightModelShader.hlsl"));

            // Defaults
            floorMat.Transparency = Transparency.Blend;
            floorMat.SetVector("radius", new Vec4(1, 2, 0, 0));
            floorMat.QueueOffset = -11;
            bunnyMat[MatParamName.DiffuseTex] = carpet;
            bunny.Visuals[0].Material = bunnyMat;
        }
    }
}
