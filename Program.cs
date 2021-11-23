using StereoKit;
using System;
using System.Collections.Generic;

namespace SK_Shader_info
{
    class Program
    {
        static void Main(string[] args)
        {
            // Initialize StereoKit
            if (!SK.Initialize(new SKSettings
            {
                appName = "SK LookDev",
                assetsFolder = "Assets",
                logFilter = LogLevel.Info,
                displayPreference = DisplayMode.Flatscreen,
            }))
                Environment.Exit(1);

            // initialize assets and environment
            Light.Initialize();
            UIUnlit.Initialize();
            Assets.Load();
            Material bunnyMat = Assets.ChangeLightType(1);

            // Poses and Variables
            Matrix guardian = Matrix.Identity;
            Pose ctrlWinPose = new Pose(-0.5f, 1f, 0f, Quat.LookDir(0, 0, 1));
            Pose dirLightPose = new Pose(0.15f, 0.2f, 0, Quat.LookDir(0, 0, 1));
            Pose pointLightPose = new Pose(0.2f, 0.2f, 0, Quat.LookDir(0, 0, 1));
            Pose spotLightPose = new Pose(0.25f, 0.2f, 0f, Quat.LookDir(0, 0, 1));
            Pose capsuleLightPose = new Pose(0.3f, 0.2f, 0f, Quat.LookDir(0, 0, -1));
            int lightTypeButton = 1;

            // Shader vectors
            var AmbientUp = 0f;
            var AmbientDown = 0f;
            var PointLightPosition = new Vec4(1f, 1f, 1f, 0f);
            var LightRangeRcp = 0.1f;
            var LightColor = new Vec4(1, 1, 1, 1);
            var DirToLight = new Vec4(1, 1, 1, 0);
            var specExp = 40f;
            var specIntensity = 5f;
            var SpotLightPos = new Vec4(1f, 1f, 1f, 0f);
            var SpotDirToLight = new Vec4(1f, 1f, 1f, 0f);
            var SpotCosOuterCone = 0.90f;
            var SpotCosInnerConeRcp = 60f;
            var CapsuleLightPos = new Vec4(1f, 1f, 1f, 0f);
            var CapsuleLightDir = new Vec4(1f, 1f, 1f, 0f);
            var CapsuleLightLen = 0.25f;
            var tex_scale = 1f;

            // Core loop
            while (SK.Step(() =>
            {
                if (World.HasBounds)
                {
                    Vec2 s = World.BoundsSize / 2;
                    guardian = World.BoundsPose.ToMatrix();
                    Vec3 tl = guardian.Transform(new Vec3(s.x, 0, s.y));
                    Vec3 br = guardian.Transform(new Vec3(-s.x, 0, -s.y));
                    Vec3 tr = guardian.Transform(new Vec3(-s.x, 0, s.y));
                    Vec3 bl = guardian.Transform(new Vec3(s.x, 0, -s.y));

                    Lines.Add(tl, tr, Color.White, 0.5f * U.cm);
                    Lines.Add(bl, br, Color.White, 0.5f * U.cm);
                    Lines.Add(tl, bl, Color.White, 0.5f * U.cm);
                    Lines.Add(tr, br, Color.White, 0.5f * U.cm);
                }
                else
                {
                    guardian = Matrix.T(V.XYZ(0, -1f, 0.5f));
                }

                // Push the world space forward a bit
                Hierarchy.Push(guardian * Matrix.T(V.XYZ(0, 0, -0.4f)));

                Renderer.Add(Assets.floorMesh, Matrix.TR(new Vec3(0, 0, 0), Quat.Identity), Color.White);
                Assets.bunny.Draw(Matrix.TR(V.XYZ(0, 0.6f, 0f), Quat.Identity));

                // UI Stuff
                UI.WindowBegin("Control", ref ctrlWinPose);
                UI.Label("Light Type");
                if (UI.Radio("Direct", lightTypeButton == 1))
                {
                    lightTypeButton = 1;
                    bunnyMat = Assets.ChangeLightType(1);
                };
                UI.SameLine();
	            if (UI.Radio("Point", lightTypeButton == 2))
                {
                    lightTypeButton = 2;
                    bunnyMat = Assets.ChangeLightType(2);
                }
                UI.SameLine();
                if (UI.Radio("Spot", lightTypeButton == 3))
                {
                    lightTypeButton = 3;
                    bunnyMat = Assets.ChangeLightType(3);
                }
                UI.SameLine();
                if (UI.Radio("Capsule", lightTypeButton == 4))
                {
                    lightTypeButton = 4;
                    bunnyMat = Assets.ChangeLightType(4);
                }
                //UI.SameLine();
                //if (UI.Radio("UberShader", lightTypeButton == 5))
                //{
                //    lightTypeButton = 5;
                //    bunnyMat = Assets.ChangeLightType(5);
                //}
                UI.Label("Ambient Light Top: " + AmbientUp.ToString("n2"));
                UI.SameLine();
                UI.Label("  Ambient Light Bottom: " + AmbientDown.ToString("n2"));
                UI.HSlider("AmbientUp", ref AmbientUp, 0f, 5f, 0.1f, 0.2f);
                UI.SameLine();
                UI.HSlider("AmbientDown", ref AmbientDown, 0f, 5f, 0.1f, 0.2f);
                UI.Label("Specular Exponent: " + specExp.ToString("n2"));
                UI.SameLine();
                UI.Label("  Specular Intensity: " + specIntensity.ToString("n2"));
                UI.HSlider("specExp", ref specExp, 1f, 255f, 0.01f, 0.2f);
                UI.SameLine();
                UI.HSlider("specintense", ref specIntensity, 1f, 50f, 0.01f, 0.2f);
                UI.Label("Light Range Attenuation: " + LightRangeRcp.ToString("n2"));
                UI.SameLine();
                UI.Label("  Texture Scale: " + tex_scale.ToString("n2"));
                UI.HSlider("PointLightRangeRcp", ref LightRangeRcp, 0.1f, 10f, 0.01f, 0.2f);
                UI.SameLine();
                UI.HSlider("tex_scale", ref tex_scale, 0.1f, 5f, 0.1f, 0.2f);
                UI.Label("Inner Cone Angle: " + SpotCosInnerConeRcp.ToString("n2"));
                UI.SameLine();
                UI.Label("  Outer Cone Angle: " + SpotCosOuterCone.ToString("n2"));
                UI.HSlider("InnerCone", ref SpotCosInnerConeRcp, 0f, 100f, 1f, 0.2f);
                UI.SameLine();
                UI.HSlider("Outer Cone Angle", ref SpotCosOuterCone, 0f, 5f, 0.1f, 0.2f);
                UI.Label("Capsule Light Length: " + CapsuleLightLen.ToString("n2"));
                UI.HSlider("Capsule Length", ref CapsuleLightLen, 0f, 5f, 0.1f, 0.2f);
                UI.WindowEnd();

                // Set the world space back to default
                Hierarchy.Pop();

                // Handles
                UI.Handle("dirLight", ref dirLightPose, Assets.dirLightModel.Bounds);
                UI.Handle("pointLight", ref pointLightPose, Assets.pointLightModel.Bounds);
                UI.Handle("spotLight", ref spotLightPose, Assets.spotLightModel.Bounds);
                UI.Handle("capsuleLight", ref capsuleLightPose, Assets.capsuleLightModel.Bounds);

                // Draws
                Renderer.Add(Assets.dirLightModel, dirLightPose.ToMatrix());
                Renderer.Add(Assets.pointLightModel, pointLightPose.ToMatrix());
                Renderer.Add(Assets.spotLightModel, spotLightPose.ToMatrix());
                Renderer.Add(Assets.capsuleLightModel, capsuleLightPose.ToMatrix(V.XYZ(1f, 1f, CapsuleLightLen)));
                //Renderer.Add(Model.FromMesh(Default.MeshQuad, Assets.bunnyMat), Matrix.TR(V.XYZ(0, -1.1f, 0), Quat.LookDir(V.XYZ(0, 1, 0))));

                // Scene driven vars for shaders
                Vec3 dirLightRotation = dirLightPose.orientation * Vec3.Forward;
                Vec3 pointLightPosition = pointLightPose.position;
                Vec3 spotLightPosition = spotLightPose.position;
                Vec3 spotLightDirection = spotLightPose.orientation * Vec3.Forward;
                Vec3 capsuleLightPosition = capsuleLightPose.position;
                Vec3 capsuleLightDirection = capsuleLightPose.orientation * Vec3.Forward;

                // Update shader vectors based on UI values
                bunnyMat.SetVector("AmbientUp", new Vec4(AmbientUp, AmbientUp, AmbientUp, 0));
                bunnyMat.SetVector("AmbientDown", new Vec4(AmbientDown, AmbientDown, AmbientDown, 0));
                bunnyMat.SetFloat("LightRangeRcp", LightRangeRcp);
                bunnyMat.SetColor("LightColor", Color.White);
                bunnyMat.SetFloat("specExp", specExp);
                bunnyMat.SetFloat("specIntensity", specIntensity);
                bunnyMat.SetVector("DirToLight", new Vec4(dirLightRotation, 0));
                bunnyMat.SetVector("PointLightPosition", new Vec4(pointLightPosition, 0));
                bunnyMat.SetVector("SpotLightPos", new Vec4(spotLightPosition, 0));
                bunnyMat.SetVector("SpotDirToLight", new Vec4(spotLightDirection, 0));
                bunnyMat.SetFloat("SpotCosOuterCone", SpotCosOuterCone);
                bunnyMat.SetFloat("SpotCosInnerConeRcp", SpotCosInnerConeRcp);
                bunnyMat.SetVector("CapsuleLightPos", new Vec4(capsuleLightPosition, 0));
                bunnyMat.SetVector("CapsuleLightDir", new Vec4(capsuleLightDirection, 0));
                bunnyMat.SetFloat("CapsuleLightLen", CapsuleLightLen);
                bunnyMat.SetFloat("tex_scale", tex_scale);
            })) ;
            SK.Shutdown();
        }
    }
}
