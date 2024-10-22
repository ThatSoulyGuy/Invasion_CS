using Invasion.Render;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.Json.Serialization;
using System.Text.Json;
using Invasion.Math;

namespace Invasion.Model
{
    public class Model
    {
        [JsonPropertyName("texture_size")]
        public double[] TextureSize { get; set; } = Array.Empty<double>();

        [JsonPropertyName("elements")]
        public List<Element> Elements { get; set; } = new();
    }

    public class Element
    {
        [JsonPropertyName("from")]
        public double[] From { get; set; } = Array.Empty<double>();

        [JsonPropertyName("to")]
        public double[] To { get; set; } = Array.Empty<double>();

        [JsonPropertyName("rotation")]
        public Rotation? Rotation { get; set; }

        [JsonPropertyName("faces")]
        public Dictionary<string, Face> Faces { get; set; } = new();
    }

    public class Rotation
    {
        [JsonPropertyName("angle")]
        public double Angle { get; set; }

        [JsonPropertyName("axis")]
        public string Axis { get; set; } = string.Empty;

        [JsonPropertyName("origin")]
        public double[] Origin { get; set; } = Array.Empty<double>();
    }

    public class Face
    {
        [JsonPropertyName("uv")]
        public double[] UV { get; set; } = Array.Empty<double>();

        [JsonPropertyName("texture")]
        public string Texture { get; set; } = string.Empty;
    }

    public static class ModelLoader
    {
        public static void LoadModel(string jsonContent, out List<Vertex> vertices, out List<uint> indices)
        {
            var model = JsonSerializer.Deserialize<Model>(jsonContent);

            vertices = [];
            indices = [];

            uint vertexOffset = 0;

            double textureWidth = model!.TextureSize[0];
            double textureHeight = model.TextureSize[1];

            foreach (var element in model.Elements)
            {
                Vector3d from = new(element.From[0], element.From[1], element.From[2]);
                Vector3d to = new(element.To[0], element.To[1], element.To[2]);

                Vector3d[] positions =
                {
                    new Vector3d(from.X, from.Y, from.Z),
                    new Vector3d(to.X, from.Y, from.Z),
                    new Vector3d(to.X, to.Y, from.Z),
                    new Vector3d(from.X, to.Y, from.Z),
                    new Vector3d(from.X, from.Y, to.Z),
                    new Vector3d(to.X, from.Y, to.Z),
                    new Vector3d(to.X, to.Y, to.Z),
                    new Vector3d(from.X, to.Y, to.Z),
                };

                if (element.Rotation != null)
                {
                    var rotation = element.Rotation;

                    Vector3d origin = new(rotation.Origin[0], rotation.Origin[1], rotation.Origin[2]);

                    double angle = rotation.Angle;

                    Vector3d axis = rotation.Axis switch
                    {
                        "x" => Vector3d.UnitX,
                        "y" => Vector3d.UnitY,
                        "z" => Vector3d.UnitZ,
                        _ => Vector3d.Zero
                    };

                    Matrix4x4 rotMatrix = Matrix4x4.CreateFromAxisAngle(axis, MathF.PI * (float)angle / 180.0f);

                    for (int i = 0; i < positions.Length; i++)
                    {
                        Vector3d position = positions[i] - origin;

                        position = Vector3.Transform(position, rotMatrix);

                        positions[i] = position + origin;
                    }
                }

                foreach (var kvp in element.Faces)
                {
                    string faceName = kvp.Key;
                    Face face = kvp.Value;

                    GetFaceVerticesAndNormal(faceName, positions, out Vector3d[] facePositions, out Vector3d normal);

                    Vector2d[] uvs = MapUVs(face.UV, textureWidth, textureHeight);

                    for (int i = 0; i < 4; i++)
                    {
                        Vertex vertex = new Vertex
                        {
                            Position = facePositions[i],
                            Color = new Vector3(1, 1, 1),
                            Normal = normal,
                            UVs = uvs[i]
                        };

                        vertices.Add(vertex);
                    }

                    indices.Add(vertexOffset + 0);
                    indices.Add(vertexOffset + 2);
                    indices.Add(vertexOffset + 1);
                    
                    indices.Add(vertexOffset + 0);
                    indices.Add(vertexOffset + 3);
                    indices.Add(vertexOffset + 2);
                    
                    vertexOffset += 4;
                }
            }
        }

        private static void GetFaceVerticesAndNormal(string faceName, Vector3d[] positions, out Vector3d[] facePositions, out Vector3d normal)
        {
            facePositions = new Vector3d[4];
            normal = Vector3d.Zero;

            switch (faceName)
            {
                case "north":
                    facePositions[0] = positions[4]; // BL
                    facePositions[1] = positions[5]; // BR
                    facePositions[2] = positions[1]; // TR
                    facePositions[3] = positions[0]; // TL
                    normal = new Vector3d(0, 0, -1);
                    break;
                case "south":
                    facePositions[0] = positions[6]; // BL
                    facePositions[1] = positions[7]; // BR
                    facePositions[2] = positions[3]; // TR
                    facePositions[3] = positions[2]; // TL
                    normal = new Vector3d(0, 0, 1);
                    break;
                case "west":
                    facePositions[0] = positions[7]; // BL
                    facePositions[1] = positions[4]; // BR
                    facePositions[2] = positions[0]; // TR
                    facePositions[3] = positions[3]; // TL
                    normal = new Vector3d(-1, 0, 0);
                    break;
                case "east":
                    facePositions[0] = positions[5]; // BL
                    facePositions[1] = positions[6]; // BR
                    facePositions[2] = positions[2]; // TR
                    facePositions[3] = positions[1]; // TL
                    normal = new Vector3d(1, 0, 0);
                    break;
                case "up":
                    facePositions[0] = positions[7]; // BL
                    facePositions[1] = positions[6]; // BR
                    facePositions[2] = positions[5]; // TR
                    facePositions[3] = positions[4]; // TL
                    normal = new Vector3d(0, 1, 0);
                    break;
                case "down":
                    facePositions[0] = positions[0]; // BL
                    facePositions[1] = positions[1]; // BR
                    facePositions[2] = positions[2]; // TR
                    facePositions[3] = positions[3]; // TL
                    normal = new Vector3d(0, -1, 0);
                    break;
                default:
                    throw new Exception($"Unknown face name: {faceName}");
            }
        }

        private static Vector2d[] MapUVs(double[] uv, double textureWidth, double textureHeight)
        {
            Vector2d[] uvs =
            [
                new Vector2d(uv[0], uv[3]),
                new Vector2d(uv[2], uv[3]),
                new Vector2d(uv[2], uv[1]),
                new Vector2d(uv[0], uv[1]),
            ];

            for (int i = 0; i < 4; i++)
            {
                uvs[i].X /= textureWidth;
                uvs[i].Y /= textureHeight;
            }

            return uvs;
        }
    }
}