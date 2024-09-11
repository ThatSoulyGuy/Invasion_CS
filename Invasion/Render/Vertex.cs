using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Invasion.Render
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex(Vector3 position, Vector3 color, Vector3 normal, Vector2 uvs)
    {
        public Vector3 Position = position;
        public Vector3 Color = color;
        public Vector3 Normal = normal;
        public Vector2 UVs = uvs;

        public static InputElementDescription[] InputElements { get; } =
        [
            new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
            new InputElementDescription("COLOR", 0, Format.R32G32B32_Float, 12, 0, InputClassification.PerVertexData, 0),
            new InputElementDescription("NORMAL", 0, Format.R32G32B32_Float, 24, 0, InputClassification.PerVertexData, 0),
            new InputElementDescription("TEXCOORD", 0, Format.R32G32_Float, 36, 0, InputClassification.PerVertexData, 0)
        ];
    }
}
