using System;
using System.Runtime.InteropServices;

namespace Invasion.Math
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2i
    {
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;

        public static Vector2i Zero => new Vector2i(0);
        public static Vector2i One => new Vector2i(1);

        public static Vector2i UnitX => new Vector2i(1, 0);
        public static Vector2i UnitY => new Vector2i(0, 1);

        public Vector2i() { }

        public Vector2i(int s)
        {
            X = s;
            Y = s;
        }

        public Vector2i(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Vector2i(Vector2i v)
        {
            X = v.X;
            Y = v.Y;
        }

        public static Vector2i operator +(Vector2i a, Vector2i b)
        {
            return new Vector2i(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2i operator -(Vector2i a, Vector2i b)
        {
            return new Vector2i(a.X - b.X, a.Y - b.Y);
        }

        public static Vector2i operator *(Vector2i a, Vector2i b)
        {
            return new Vector2i(a.X * b.X, a.Y * b.Y);
        }

        public static Vector2i operator /(Vector2i a, Vector2i b)
        {
            return new Vector2i(a.X / b.X, a.Y / b.Y);
        }

        public static Vector2i operator +(Vector2i a, int b)
        {
            return new Vector2i(a.X + b, a.Y + b);
        }

        public static Vector2i operator -(Vector2i a, int b)
        {
            return new Vector2i(a.X - b, a.Y - b);
        }

        public static Vector2i operator *(Vector2i a, int b)
        {
            return new Vector2i(a.X * b, a.Y * b);
        }

        public static Vector2i operator /(Vector2i a, int b)
        {
            return new Vector2i(a.X / b, a.Y / b);
        }

        public static Vector2i operator +(int a, Vector2i b)
        {
            return new Vector2i(a + b.X, a + b.Y);
        }

        public static Vector2i operator -(int a, Vector2i b)
        {
            return new Vector2i(a - b.X, a - b.Y);
        }

        public static Vector2i operator *(int a, Vector2i b)
        {
            return new Vector2i(a * b.X, a * b.Y);
        }

        public static Vector2i operator /(int a, Vector2i b)
        {
            return new Vector2i(a / b.X, a / b.Y);
        }

        public static Vector2i operator -(Vector2i a)
        {
            return new Vector2i(-a.X, -a.Y);
        }

        public static explicit operator Vector2f(Vector2i v)
        {
            return new Vector2f(v.X, v.Y);
        }

        public static explicit operator Vector2d(Vector2i v)
        {
            return new Vector2d(v.X, v.Y);
        }

        public static bool operator ==(Vector2i a, Vector2i b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Vector2i a, Vector2i b)
        {
            return a.X != b.X || a.Y != b.Y;
        }

        public int Length()
        {
            return (int)System.Math.Sqrt(X * X + Y * Y);
        }

        public int Dot()
        {
            return X * X + Y * Y;
        }

        public int Cross()
        {
            return X * Y;
        }

        public Vector2i Normalize()
        {
            return this / Length();
        }

        public Vector2i Lerp(Vector2i b, int t)
        {
            return this * (1 - t) + b * t;
        }

        public override bool Equals(object? obj)
        {
            return obj is Vector2i i &&
                   X == i.X &&
                   Y == i.Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2f
    {
        public float X { get; set; } = 0;
        public float Y { get; set; } = 0;

        public static Vector2f Zero => new Vector2f(0);
        public static Vector2f One => new Vector2f(1);

        public static Vector2f UnitX => new Vector2f(1, 0);
        public static Vector2f UnitY => new Vector2f(0, 1);

        public Vector2f() { }

        public Vector2f(float s)
        {
            X = s;
            Y = s;
        }

        public Vector2f(float x, float y)
        {
            X = x;
            Y = y;
        }

        public Vector2f(Vector2f v)
        {
            X = v.X;
            Y = v.Y;
        }

        public static Vector2f operator +(Vector2f a, Vector2f b)
        {
            return new Vector2f(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2f operator -(Vector2f a, Vector2f b)
        {
            return new Vector2f(a.X - b.X, a.Y - b.Y);
        }

        public static Vector2f operator *(Vector2f a, Vector2f b)
        {
            return new Vector2f(a.X * b.X, a.Y * b.Y);
        }

        public static Vector2f operator /(Vector2f a, Vector2f b)
        {
            return new Vector2f(a.X / b.X, a.Y / b.Y);
        }

        public static Vector2f operator +(Vector2f a, float b)
        {
            return new Vector2f(a.X + b, a.Y + b);
        }

        public static Vector2f operator -(Vector2f a, float b)
        {
            return new Vector2f(a.X - b, a.Y - b);
        }

        public static Vector2f operator *(Vector2f a, float b)
        {
            return new Vector2f(a.X * b, a.Y * b);
        }

        public static Vector2f operator /(Vector2f a, float b)
        {
            return new Vector2f(a.X / b, a.Y / b);
        }

        public static Vector2f operator +(float a, Vector2f b)
        {
            return new Vector2f(a + b.X, a + b.Y);
        }

        public static Vector2f operator -(float a, Vector2f b)
        {
            return new Vector2f(a - b.X, a - b.Y);
        }

        public static Vector2f operator *(float a, Vector2f b)
        {
            return new Vector2f(a * b.X, a * b.Y);
        }

        public static Vector2f operator /(float a, Vector2f b)
        {
            return new Vector2f(a / b.X, a / b.Y);
        }

        public static Vector2f operator -(Vector2f a)
        {
            return new Vector2f(-a.X, -a.Y);
        }

        public static explicit operator Vector2i(Vector2f v)
        {
            return new Vector2i((int)v.X, (int)v.Y);
        }

        public static explicit operator Vector2d(Vector2f v)
        {
            return new Vector2d(v.X, v.Y);
        }

        public static bool operator ==(Vector2f a, Vector2f b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Vector2f a, Vector2f b)
        {
            return a.X != b.X || a.Y != b.Y;
        }

        public float Length()
        {
            return (float)System.Math.Sqrt(X * X + Y * Y);
        }

        public float Dot()
        {
            return X * X + Y * Y;
        }

        public float Cross()
        {
            return X * Y;
        }

        public Vector2f Normalize()
        {
            return this / Length();
        }

        public Vector2f Lerp(Vector2f b, float t)
        {
            return this * (1 - t) + b * t;
        }

        public Vector2f Lerp(Vector2f b, float t, float dt)
        {
            return this * (1 - dt) + b * dt;
        }

        public Vector2f Lerp(Vector2f b, Vector2f t)
        {
            return this * (1 - t) + b * t;
        }

        public Vector2f Lerp(Vector2f b, Vector2f t, Vector2f dt)
        {
            return this * (1 - dt) + b * dt;
        }

        public static Vector2f Lerp(Vector2f a, Vector2f b, float t)
        {
            return a * (1 - t) + b * t;
        }

        public override bool Equals(object? obj)
        {
            return obj is Vector2f i &&
                   X == i.X &&
                   Y == i.Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2d
    {
        public double X { get; set; } = 0;
        public double Y { get; set; } = 0;

        public static Vector2d Zero => new Vector2d(0);
        public static Vector2d One => new Vector2d(1);

        public static Vector2d UnitX => new Vector2d(1, 0);
        public static Vector2d UnitY => new Vector2d(0, 1);

        public Vector2d() { }

        public Vector2d(double s)
        {
            X = s;
            Y = s;
        }

        public Vector2d(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Vector2d(Vector2d v)
        {
            X = v.X;
            Y = v.Y;
        }

        public static Vector2d operator +(Vector2d a, Vector2d b)
        {
            return new Vector2d(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2d operator -(Vector2d a, Vector2d b)
        {
            return new Vector2d(a.X - b.X, a.Y - b.Y);
        }

        public static Vector2d operator *(Vector2d a, Vector2d b)
        {
            return new Vector2d(a.X * b.X, a.Y * b.Y);
        }

        public static Vector2d operator /(Vector2d a, Vector2d b)
        {
            return new Vector2d(a.X / b.X, a.Y / b.Y);
        }

        public static Vector2d operator +(Vector2d a, double b)
        {
            return new Vector2d(a.X + b, a.Y + b);
        }

        public static Vector2d operator -(Vector2d a, double b)
        {
            return new Vector2d(a.X - b, a.Y - b);
        }

        public static Vector2d operator *(Vector2d a, double b)
        {
            return new Vector2d(a.X * b, a.Y * b);
        }

        public static Vector2d operator /(Vector2d a, double b)
        {
            return new Vector2d(a.X / b, a.Y / b);
        }

        public static Vector2d operator +(double a, Vector2d b)
        {
            return new Vector2d(a + b.X, a + b.Y);
        }

        public static Vector2d operator -(double a, Vector2d b)
        {
            return new Vector2d(a - b.X, a - b.Y);
        }

        public static Vector2d operator *(double a, Vector2d b)
        {
            return new Vector2d(a * b.X, a * b.Y);
        }

        public static Vector2d operator /(double a, Vector2d b)
        {
            return new Vector2d(a / b.X, a / b.Y);
        }

        public static Vector2d operator -(Vector2d a)
        {
            return new Vector2d(-a.X, -a.Y);
        }

        public static implicit operator Vector2i(Vector2d v)
        {
            return new Vector2i((int)v.X, (int)v.Y);
        }

        public static implicit operator Vector2f(Vector2d v)
        {
            return new Vector2f((float)v.X, (float)v.Y);
        }

        public static bool operator ==(Vector2d a, Vector2d b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Vector2d a, Vector2d b)
        {
            return a.X != b.X || a.Y != b.Y;
        }

        public double Length()
        {
            return System.Math.Sqrt(X * X + Y * Y);
        }

        public double Dot()
        {
            return X * X + Y * Y;
        }

        public double Cross()
        {
            return X * Y;
        }

        public Vector2d Normalize()
        {
            return this / Length();
        }

        public Vector2d Lerp(Vector2d b, double t)
        {
            return this * (1 - t) + b * t;
        }

        public override bool Equals(object? obj)
        {
            return obj is Vector2d i &&
                   X == i.X &&
                   Y == i.Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }
}