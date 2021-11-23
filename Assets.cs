using StereoKit;

namespace SK_Shader_info

{
    enum LightType
    {

    }

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
        public static Material directMat;
        public static Material pointMat;
        public static Material spotMat;
        public static Material capsuleMat;
        public static Material uberMat;

        static Tex carpet;

        public static void Load()
        {
            // Textures
            carpet = Tex.FromFile("carpet.jpg");

            // Materials
            floorMat = new Material(Shader.FromFile("floor.hlsl"));
            directMat = new Material(Shader.FromFile("direct.hlsl"));
            pointMat = new Material(Shader.FromFile("point.hlsl"));
            spotMat = new Material(Shader.FromFile("spot.hlsl"));
            capsuleMat = new Material(Shader.FromFile("capsule.hlsl"));
            //uberMat = new Material(Shader.FromFile("uberShader.hlsl"));

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
        }

        public static Material ChangeLightType(int type)
        {
            if (type == 1)
            {
                bunny.Visuals[0].Material = directMat;
                bunny.Visuals[0].Material[MatParamName.DiffuseTex] = carpet;
                return directMat;
            }
            else if (type == 2)
            {
                bunny.Visuals[0].Material = pointMat;
                bunny.Visuals[0].Material[MatParamName.DiffuseTex] = carpet;
                return pointMat;
            }
            else if (type == 3)
            {
                bunny.Visuals[0].Material = spotMat;
                bunny.Visuals[0].Material[MatParamName.DiffuseTex] = carpet;
                return spotMat;
            }
            else if (type == 4)
            {
                bunny.Visuals[0].Material = capsuleMat;
                bunny.Visuals[0].Material[MatParamName.DiffuseTex] = carpet;
                return capsuleMat;
            }
            bunny.Visuals[0].Material = directMat;
            bunny.Visuals[0].Material[MatParamName.DiffuseTex] = carpet;
            return directMat;
        }
    }
}
