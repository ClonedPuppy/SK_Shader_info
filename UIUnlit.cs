using StereoKit;

namespace SK_Shader_info
{
    class UIUnlit
    {
        static public void Initialize()
        {
            // make default UI material fully emissive
            Default.MaterialUI.Shader = Shader.Unlit;
            Default.MaterialUI[MatParamName.DiffuseTex] = Default.Tex;
            Material.Find("default/material_ui_quadrant").Shader = Shader.FromFile("shader_builtin_ui_quadrant_unlit.hlsl");
            Material.Find("default/material_ui_quadrant")[MatParamName.DiffuseTex] = Default.Tex;
            Default.MaterialHand.Shader = Shader.Unlit;
            Default.MaterialHand[MatParamName.DiffuseTex] = Default.Tex;

            // Hand gradient
            ColorizeFingers(16,
                    new Gradient(new GradientKey(new Color(0.75f, 0.75f, 0.75f, 0.75f), 1)),
                    new Gradient(
                        new GradientKey(new Color(.4f, .4f, .4f, 0), 0),
                        new GradientKey(new Color(.6f, .6f, .6f, 0), 0.4f),
                        new GradientKey(new Color(.8f, .8f, .8f, 1), 0.55f),
                        new GradientKey(new Color(0.75f, 0.75f, 0.75f, 0.75f), 0.75f)));
        }

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

        public static TextStyle SetFont()
        {
            TextStyle style = Text.MakeStyle(
                Font.FromFile("Assets/Maia.ttf") ?? Default.Font,
                1f * U.cm,
                Color.HSV(1f, 0f, 1f));

            return style;
        }
    }
}
