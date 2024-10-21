using Invasion.ECS;
using Invasion.Math;
using Invasion.Render;
using Invasion.World;
using System.Collections.Generic;

namespace Invasion.Entity.Model
{
    public readonly struct Cube(Vector3f position, Vector3f size, Vector2f[] uvs)
    {
        public Vector3f Position { get; } = position;
        public Vector3f Size { get; } = size;
        public Vector2f[] UVs { get; } = uvs;

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

        public List<Cube> Cubes { get; init; } = [];

        public UIMesh Mesh => GameObject.GetComponent<UIMesh>();

        private ModelPart() { }

        public void Generate()
        {
            foreach (Cube cube in Cubes)
                AddCubeToMesh(cube);

            Mesh.Generate();
        }

        public void AddCube(Vector3f position, Vector3f size, Vector2f[] uvs)
        {
            AddCube(new(position, size, uvs));
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

            Vector2f[,] allUVs = new Vector2f[6, 4];

            for (int i = 0; i < 6; i++)
            {
                allUVs[i, 0] = cube.UVs[i * 4];
                allUVs[i, 1] = cube.UVs[i * 4 + 1];
                allUVs[i, 2] = cube.UVs[i * 4 + 2];
                allUVs[i, 3] = cube.UVs[i * 4 + 3];
            }

            Vector2f[] frontRawUVs = { allUVs[0, 0], allUVs[0, 1], allUVs[0, 2], allUVs[0, 3] };
            Vector2f[] backRawUVs = { allUVs[1, 0], allUVs[1, 1], allUVs[1, 2], allUVs[1, 3] };
            Vector2f[] rightRawUVs = { allUVs[2, 0], allUVs[2, 1], allUVs[2, 2], allUVs[2, 3] };
            Vector2f[] leftRawUVs = { allUVs[3, 0], allUVs[3, 1], allUVs[3, 2], allUVs[3, 3] };
            Vector2f[] topRawUVs = { allUVs[4, 0], allUVs[4, 1], allUVs[4, 2], allUVs[4, 3] };
            Vector2f[] bottomRawUVs = { allUVs[5, 0], allUVs[5, 1], allUVs[5, 2], allUVs[5, 3] };

            Vector2f[] frontUVs = GameObject.GetComponent<TextureAtlas>().GetTextureCoordinates(TextureName);
            Vector2f[] backUVs = GameObject.GetComponent<TextureAtlas>().GetTextureCoordinates(TextureName);
            Vector2f[] rightUVs = GameObject.GetComponent<TextureAtlas>().GetTextureCoordinates(TextureName);
            Vector2f[] leftUVs = GameObject.GetComponent<TextureAtlas>().GetTextureCoordinates(TextureName);
            Vector2f[] topUVs = GameObject.GetComponent<TextureAtlas>().GetTextureCoordinates(TextureName);
            Vector2f[] bottomUVs = GameObject.GetComponent<TextureAtlas>().GetTextureCoordinates(TextureName);

            Vector3f vertexColor = new(1.0f, 1.0f, 1.0f);

            AddFace(Mesh, corners[0], corners[1], corners[5], corners[4], normals[0], vertexColor, frontUVs);

            AddFace(Mesh, corners[2], corners[3], corners[7], corners[6], normals[1], vertexColor, backUVs);

            AddFace(Mesh, corners[1], corners[2], corners[6], corners[5], normals[2], vertexColor, rightUVs);

            AddFace(Mesh, corners[3], corners[0], corners[4], corners[7], normals[3], vertexColor, leftUVs);

            AddFace(Mesh, corners[4], corners[5], corners[6], corners[7], normals[4], vertexColor, topUVs);

            AddFace(Mesh, corners[0], corners[3], corners[2], corners[1], normals[5], vertexColor, bottomUVs);
        }

        private void AddFace(UIMesh mesh, Vector3f v0, Vector3f v1, Vector3f v2, Vector3f v3, Vector3f normal, Vector3f color, Vector2f[] uvs)
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
