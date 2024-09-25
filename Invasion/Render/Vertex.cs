using Invasion.Math;
using System.Runtime.InteropServices;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Invasion.Render
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex(Vector3f position, Vector3f color, Vector3f normal, Vector2f uvs)
    {
        public Vector3f Position = position;
        public Vector3f Color = color;
        public Vector3f Normal = normal;
        public Vector2f UVs = uvs;

        public static InputElementDescription[] InputElements { get; } =
        [
            new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
            new InputElementDescription("COLOR", 0, Format.R32G32B32_Float, 12, 0, InputClassification.PerVertexData, 0),
            new InputElementDescription("NORMAL", 0, Format.R32G32B32_Float, 24, 0, InputClassification.PerVertexData, 0),
            new InputElementDescription("TEXCOORD", 0, Format.R32G32_Float, 36, 0, InputClassification.PerVertexData, 0)
        ];
    }
}
