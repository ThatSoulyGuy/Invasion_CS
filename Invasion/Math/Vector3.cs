using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Invasion.Math
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3i
    {
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;
        public int Z { get; set; } = 0;

        public Vector3i() { }

        public Vector3i(int s)
        {
            X = s;
            Y = s;
            Z = s;
        }

        public Vector3i(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3i(Vector3i v)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
        }

        public static Vector3i operator +(Vector3i a, Vector3i b)
        {
            return new Vector3i(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Vector3i operator -(Vector3i a, Vector3i b)
        {
            return new Vector3i(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static Vector3i operator *(Vector3i a, Vector3i b)
        {
            return new Vector3i(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        public static Vector3i operator /(Vector3i a, Vector3i b)
        {
            return new Vector3i(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
        }

        public static Vector3i operator +(Vector3i a, int b)
        {
            return new Vector3i(a.X + b, a.Y + b, a.Z + b);
        }

        public static Vector3i operator -(Vector3i a, int b)
        {
            return new Vector3i(a.X - b, a.Y - b, a.Z - b);
        }

        public static Vector3i operator *(Vector3i a, int b)
        {
            return new Vector3i(a.X * b, a.Y * b, a.Z * b);
        }

        public static Vector3i operator /(Vector3i a, int b)
        {
            return new Vector3i(a.X / b, a.Y / b, a.Z / b);
        }

        public static Vector3i operator +(int a, Vector3i b)
        {
            return new Vector3i(a + b.X, a + b.Y, a + b.Z);
        }

        public static Vector3i operator -(int a, Vector3i b)
        {
            return new Vector3i(a - b.X, a - b.Y, a - b.Z);
        }

        public static Vector3i operator *(int a, Vector3i b)
        {
            return new Vector3i(a * b.X, a * b.Y, a * b.Z);
        }

        public static Vector3i operator /(int a, Vector3i b)
        {
            return new Vector3i(a / b.X, a / b.Y, a / b.Z);
        }

        public static implicit operator Vector3f(Vector3i v)
        {
            return new Vector3f(v.X, v.Y, v.Z);
        }

        public static implicit operator Vector3d(Vector3i v)
        {
            return new Vector3d(v.X, v.Y, v.Z);
        }

        public static implicit operator Vector3i(Vector3 v)
        {
            return new Vector3i((int)v.X, (int)v.Y, (int)v.Z);
        }

        public static implicit operator Vector3(Vector3i v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        public static bool operator ==(Vector3i a, Vector3i b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Vector3i a, Vector3i b)
        {
            return a.X != b.X || a.Y != b.Y;
        }

        public int Length()
        {
            return (int)System.Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        public int Dot(Vector3i v)
        {
            return X * v.X + Y * v.Y + Z * v.Z;
        }

        public Vector3i Cross(Vector3i v)
        {
            return new Vector3i(Y * v.Z - Z * v.Y, Z * v.X - X * v.Z, X * v.Y - Y * v.X);
        }

        public static Vector3i Normalize(Vector3i v)
        {
            return v / v.Length();
        }

        public static Vector3i Lerp(Vector3i a, Vector3i b, float t)
        {
            return new Vector3i((int)(a.X + (b.X - a.X) * t), (int)(a.Y + (b.Y - a.Y) * t), (int)(a.Z + (b.Z - a.Z) * t));
        }

        public static Vector3i TransformNormal(Vector3i vector, Matrix4x4 matrix)
        {
            return new Vector3i(
                (int)(vector.X * matrix.M11 + vector.Y * matrix.M21 + vector.Z * matrix.M31),
                (int)(vector.X * matrix.M12 + vector.Y * matrix.M22 + vector.Z * matrix.M32),
                (int)(vector.X * matrix.M13 + vector.Y * matrix.M23 + vector.Z * matrix.M33)
            );
        }

        public override bool Equals(object? obj)
        {
            return obj is Vector3i i &&
                   X == i.X &&
                   Y == i.Y &&
                   Z == i.Z;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3f
    {
        public float X { get; set; } = 0;
        public float Y { get; set; } = 0;
        public float Z { get; set; } = 0;

        public static Vector3f Zero => new(0.0f);
        public static Vector3f One => new(1.0f);
        public static Vector3f UnitX => new(1.0f, 0.0f, 0.0f);
        public static Vector3f UnitY => new(0.0f, 1.0f, 0.0f);
        public static Vector3f UnitZ => new(0.0f, 0.0f, 1.0f);

        public Vector3f() { }

        public Vector3f(float s)
        {
            X = s;
            Y = s;
            Z = s;
        }

        public Vector3f(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3f(Vector3f v)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
        }

        public static Vector3f operator +(Vector3f a, Vector3f b)
        {
            return new Vector3f(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Vector3f operator -(Vector3f a, Vector3f b)
        {
            return new Vector3f(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static Vector3f operator *(Vector3f a, Vector3f b)
        {
            return new Vector3f(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        public static Vector3f operator /(Vector3f a, Vector3f b)
        {
            return new Vector3f(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
        }

        public static Vector3f operator +(Vector3f a, float b)
        {
            return new Vector3f(a.X + b, a.Y + b, a.Z + b);
        }

        public static Vector3f operator -(Vector3f a, float b)
        {
            return new Vector3f(a.X - b, a.Y - b, a.Z - b);
        }

        public static Vector3f operator *(Vector3f a, float b)
        {
            return new Vector3f(a.X * b, a.Y * b, a.Z * b);
        }

        public static Vector3f operator /(Vector3f a, float b)
        {
            return new Vector3f(a.X / b, a.Y / b, a.Z / b);
        }

        public static Vector3f operator +(float a, Vector3f b)
        {
            return new Vector3f(a + b.X, a + b.Y, a + b.Z);
        }

        public static Vector3f operator -(float a, Vector3f b)
        {
            return new Vector3f(a - b.X, a - b.Y, a - b.Z);
        }

        public static Vector3f operator *(float a, Vector3f b)
        {
            return new Vector3f(a * b.X, a * b.Y, a * b.Z);
        }

        public static Vector3f operator /(float a, Vector3f b)
        {
            return new Vector3f(a / b.X, a / b.Y, a / b.Z);
        }

        public static implicit operator Vector3i(Vector3f v)
        {
            return new Vector3i((int)v.X, (int)v.Y, (int)v.Z);
        }

        public static implicit operator Vector3d(Vector3f v)
        {
            return new Vector3d(v.X, v.Y, v.Z);
        }

        public static implicit operator Vector3f(Vector3 v)
        {
            return new Vector3i((int)v.X, (int)v.Y, (int)v.Z);
        }

        public static implicit operator Vector3(Vector3f v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        public static bool operator ==(Vector3f a, Vector3f b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Vector3f a, Vector3f b)
        {
            return a.X != b.X || a.Y != b.Y;
        }

        public float Length()
        {
            return (float)System.Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        public float Dot(Vector3f v)
        {
            return X * v.X + Y * v.Y + Z * v.Z;
        }

        public Vector3f Cross(Vector3f v)
        {
            return new Vector3f(Y * v.Z - Z * v.Y, Z * v.X - X * v.Z, X * v.Y - Y * v.X);
        }

        public static Vector3f Normalize(Vector3f v)
        {
            return v / v.Length();
        }

        public static Vector3f Lerp(Vector3f a, Vector3f b, float t)
        {
            return new Vector3f(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t, a.Z + (b.Z - a.Z) * t);
        }

        public static Vector3f TransformNormal(Vector3f vector, Matrix4x4 matrix)
        {
            return new Vector3f(
                vector.X * matrix.M11 + vector.Y * matrix.M21 + vector.Z * matrix.M31,
                vector.X * matrix.M12 + vector.Y * matrix.M22 + vector.Z * matrix.M32,
                vector.X * matrix.M13 + vector.Y * matrix.M23 + vector.Z * matrix.M33
            );
        }

        public override bool Equals(object? obj)
        {
            return obj is Vector3f i &&
                   X == i.X &&
                   Y == i.Y &&
                   Z == i.Z;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3d
    {
        public double X { get; set; } = 0;
        public double Y { get; set; } = 0;
        public double Z { get; set; } = 0;

        public Vector3d() { }

        public Vector3d(double s)
        {
            X = s;
            Y = s;
            Z = s;
        }

        public Vector3d(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3d(Vector3d v)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
        }

        public static Vector3d operator +(Vector3d a, Vector3d b)
        {
            return new Vector3d(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Vector3d operator -(Vector3d a, Vector3d b)
        {
            return new Vector3d(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static Vector3d operator *(Vector3d a, Vector3d b)
        {
            return new Vector3d(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        public static Vector3d operator /(Vector3d a, Vector3d b)
        {
            return new Vector3d(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
        }

        public static Vector3d operator +(Vector3d a, double b)
        {
            return new Vector3d(a.X + b, a.Y + b, a.Z + b);
        }

        public static Vector3d operator -(Vector3d a, double b)
        {
            return new Vector3d(a.X - b, a.Y - b, a.Z - b);
        }

        public static Vector3d operator *(Vector3d a, double b)
        {
            return new Vector3d(a.X * b, a.Y * b, a.Z * b);
        }

        public static Vector3d operator /(Vector3d a, double b)
        {
            return new Vector3d(a.X / b, a.Y / b, a.Z / b);
        }

        public static Vector3d operator +(double a, Vector3d b)
        {
            return new Vector3d(a + b.X, a + b.Y, a + b.Z);
        }

        public static Vector3d operator -(double a, Vector3d b)
        {
            return new Vector3d(a - b.X, a - b.Y, a - b.Z);
        }

        public static Vector3d operator *(double a, Vector3d b)
        {
            return new Vector3d(a * b.X, a * b.Y, a * b.Z);
        }

        public static Vector3d operator /(double a, Vector3d b)
        {
            return new Vector3d(a / b.X, a / b.Y, a / b.Z);
        }

        public static implicit operator Vector3i(Vector3d v)
        {
            return new Vector3i((int)v.X, (int)v.Y, (int)v.Z);
        }

        public static implicit operator Vector3f(Vector3d v)
        {
            return new Vector3f((float)v.X, (float)v.Y, (float)v.Z);
        }

        public static implicit operator Vector3d(Vector3 v)
        {
            return new Vector3d(v.X, v.Y, v.Z);
        }

        public static implicit operator Vector3(Vector3d v)
        {
            return new Vector3((float)v.X, (float)v.Y, (float)v.Z);
        }

        public static bool operator ==(Vector3d a, Vector3d b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Vector3d a, Vector3d b)
        {
            return a.X != b.X || a.Y != b.Y;
        }

        public double Length()
        {
            return System.Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        public double Dot(Vector3d v)
        {
            return X * v.X + Y * v.Y + Z * v.Z;
        }

        public Vector3d Cross(Vector3d v)
        {
            return new Vector3d(Y * v.Z - Z * v.Y, Z * v.X - X * v.Z, X * v.Y - Y * v.X);
        }

        public static Vector3d Normalize(Vector3d v)
        {
            return v / v.Length();
        }

        public static Vector3d Lerp(Vector3d a, Vector3d b, float t)
        {
            return new Vector3d(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t, a.Z + (b.Z - a.Z) * t);
        }

        public static Vector3d TransformNormal(Vector3d vector, Matrix4x4 matrix)
        {
            return new Vector3d(
                vector.X * matrix.M11 + vector.Y * matrix.M21 + vector.Z * matrix.M31,
                vector.X * matrix.M12 + vector.Y * matrix.M22 + vector.Z * matrix.M32,
                vector.X * matrix.M13 + vector.Y * matrix.M23 + vector.Z * matrix.M33
            );
        }

        public override bool Equals(object? obj)
        {
            return obj is Vector3d i &&
                   X == i.X &&
                   Y == i.Y &&
                   Z == i.Z;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }
}
