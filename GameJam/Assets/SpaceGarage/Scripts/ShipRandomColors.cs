using UnityEngine;

namespace SpaceGarage
{
    public static class ShipRandomColors
    {
        public readonly struct Palette
        {
            public readonly Color Primary, Accent, Metal;
            public Palette(Color primary, Color accent, Color metal)
            { Primary = primary; Accent = accent; Metal = metal; }
            public Color ColorFor(Color original, ShipPartSlot slot)
            {
                Color color = original.maxColorComponent < 0.5f ? Metal :
                    slot == ShipPartSlot.Gun || slot == ShipPartSlot.Jet ? Accent : Primary;
                color.a = original.a;
                return color;
            }
        }

        private static readonly Palette[] palettes =
        {
            new Palette(Rgb(53,103,138), Rgb(216,155,86), Rgb(37,47,59)),
            new Palette(Rgb(54,123,121), Rgb(231,196,138), Rgb(36,49,51)),
            new Palette(Rgb(142,70,87), Rgb(219,179,121), Rgb(48,39,46)),
            new Palette(Rgb(88,117,92), Rgb(194,163,91), Rgb(38,46,41)),
            new Palette(Rgb(109,93,134), Rgb(159,198,180), Rgb(41,39,52)),
            new Palette(Rgb(183,106,65), Rgb(229,207,169), Rgb(47,41,38)),
            new Palette(Rgb(214,211,195), Rgb(66,101,130), Rgb(43,48,54)),
            new Palette(Rgb(89,108,128), Rgb(200,162,77), Rgb(36,44,54))
        };

        private static Color Rgb(byte r, byte g, byte b) => new Color32(r, g, b, 255);
        public static Palette CreatePalette() => palettes[Random.Range(0, palettes.Length)];
        public static bool IsShipSurface(Material material) =>
            material != null && material.shader != null && material.shader.name == "Toon/Toon";
        public static Color Shade(Color color, float factor)
        {
            float alpha = color.a;
            color *= factor;
            color.a = alpha;
            return color;
        }

        public static void Apply(GameObject part, Palette palette, ShipPartSlot slot)
        {
            foreach (var renderer in part.GetComponentsInChildren<Renderer>(true))
            {
                if (!(renderer is MeshRenderer) && !(renderer is SkinnedMeshRenderer)) continue;
                var materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    var material = materials[i];
                    if (!IsShipSurface(material)) continue;
                    var block = new MaterialPropertyBlock(); renderer.GetPropertyBlock(block, i);
                    Color color = palette.ColorFor(material.GetColor("_BaseColor"), slot);
                    if (material.HasProperty("_BaseColor")) block.SetColor("_BaseColor", color);
                    if (material.HasProperty("_Color")) block.SetColor("_Color", color);
                    if (material.HasProperty("_1st_ShadeColor")) block.SetColor("_1st_ShadeColor", Shade(color, 0.65f));
                    if (material.HasProperty("_2nd_ShadeColor")) block.SetColor("_2nd_ShadeColor", Shade(color, 0.4f));
                    renderer.SetPropertyBlock(block, i);
                }
            }
        }
    }
}
