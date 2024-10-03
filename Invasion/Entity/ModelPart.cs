using Invasion.ECS;
using Invasion.Math;
using Invasion.Render;
using Invasion.World;
using System.Collections.Generic;

namespace Invasion.Entity.Model
{
    public readonly struct Cube(Vector3f position, Vector3f size)
    {
        public Vector3f Position { get; } = position;
        public Vector3f Size { get; } = size;

        public readonly Vector3f Corner0 => Position;
        public readonly Vector3f Corner1 => Position + new Vector3f(Size.X, 0.0f, 0.0f);
        public readonly Vector3f Corner2 => Position + new Vector3f(Size.X, 0.0f, Size.Z);
        public readonly Vector3f Corner3 => Position + new Vector3f(0.0f, 0.0f, Size.Z);

        public readonly Vector3f Corner4 => Position + new Vector3f(0.0f, Size.Y, 0.0f);
        public readonly Vector3f Corner5 => Position + new Vector3f(Size.X, Size.Y, 0.0f);
        public readonly Vector3f Corner6 => Position + new Vector3f(Size.X, Size.Y, Size.Z);
        public readonly Vector3f Corner7 => Position + new Vector3f(0.0f, Size.Y, Size.Z);
    }

    public class ModelPart : Component
    {
        public string Name { get; init; } = string.Empty;
        public string TextureName { get; init; } = string.Empty;

        public List<Cube> Cubes { get; init; } = new();

        public Mesh Mesh => GameObject.GetComponent<Mesh>();

        private ModelPart() { }

        public void Generate()
        {
            foreach (Cube cube in Cubes)
                AddCubeToMesh(cube);

            Mesh.Generate();
        }

        public void AddCube(Vector3f position, Vector3f size)
        {
            AddCube(new Cube(position, size));
        }

        public void AddCube(Cube cube)
        {
            Cubes.Add(cube);
        }

        private void AddCubeToMesh(Cube cube)
        {
            Vector3f[] corners =
            {
                cube.Corner0, cube.Corner1, cube.Corner2, cube.Corner3,
                cube.Corner4, cube.Corner5, cube.Corner6, cube.Corner7
            };

            Vector3f[] normals =
            {
                new(0, 0, 1),
                new(0, 0, -1),
                new(1, 0, 0),
                new(-1, 0, 0),
                new(0, 1, 0),
                new(0, -1, 0),
            };

            Vector2f[] uvs = GameObject.GetComponent<TextureAtlas>().GetTextureCoordinates(TextureName);
            Vector3f vertexColor = new(1.0f, 1.0f, 1.0f);

            AddFace(Mesh, corners[0], corners[1], corners[5], corners[4], normals[0], vertexColor, uvs);

            AddFace(Mesh, corners[2], corners[3], corners[7], corners[6], normals[1], vertexColor, uvs);

            AddFace(Mesh, corners[1], corners[2], corners[6], corners[5], normals[2], vertexColor, uvs);

            AddFace(Mesh, corners[3], corners[0], corners[4], corners[7], normals[3], vertexColor, uvs);

            AddFace(Mesh, corners[4], corners[5], corners[6], corners[7], normals[4], vertexColor, uvs);

            AddFace(Mesh, corners[0], corners[3], corners[2], corners[1], normals[5], vertexColor, uvs);
        }

        private void AddFace(Mesh mesh, Vector3f v0, Vector3f v1, Vector3f v2, Vector3f v3, Vector3f normal, Vector3f color, Vector2f[] uvs)
        {
            int vertexIndexStart = mesh.Vertices.Count;

            mesh.Vertices.Add(new Vertex(v0, color, normal, uvs[0]));
            mesh.Vertices.Add(new Vertex(v1, color, normal, uvs[1]));
            mesh.Vertices.Add(new Vertex(v2, color, normal, uvs[2]));
            mesh.Vertices.Add(new Vertex(v3, color, normal, uvs[3]));

            mesh.Indices.Add((uint)vertexIndexStart + 0);
            mesh.Indices.Add((uint)vertexIndexStart + 2);
            mesh.Indices.Add((uint)vertexIndexStart + 1);

            mesh.Indices.Add((uint)vertexIndexStart + 0);
            mesh.Indices.Add((uint)vertexIndexStart + 3);
            mesh.Indices.Add((uint)vertexIndexStart + 2);
        }

        public static ModelPart Create(string name, string textureName)
        {
            return new()
            {
                Name = name,
                TextureName = textureName
            };
        }
    }
}
