using Invasion.Math;
using Invasion.Render;
using System;
using System.Numerics;

namespace Invasion.UI
{
    [Flags]
    public enum Alignment
    {
        None = 0,
        Left = 1 << 0,
        Right = 1 << 1,
        CenterX = 1 << 2,
        Top = 1 << 3,
        Bottom = 1 << 4,
        CenterY = 1 << 5
    }

    public abstract class UIElement
    {
        public string Name { get; set; } = string.Empty;

        public Vector2i ScreenDimensions => new(Renderer.Window.Bounds.Size.Width, Renderer.Window.Bounds.Size.Height);

        public Vector2f Position { get; set; } = Vector2f.Zero;
        public Vector2f AlignmentOffset { get; set; } = Vector2f.Zero;
        public Vector2f Size { get; set; } = Vector2f.Zero;

        public Alignment Alignment { get; set; } = Alignment.None;

        public UIMesh Mesh { get; set; } = null!;

        public bool IsVisible { get; set; } = true;

        public UIElement(string name, Vector2f position, Vector2f size, Alignment alignment = Alignment.None)
        {
            Name = name;
            Position = position;
            Size = size;
            Alignment = alignment;

            Mesh = UIMesh.Create(name,
            [
                new Vertex(new Vector3(0.0f, 0.0f, 0.0f), Vector3f.One, new Vector3(0, 0, 1), new Vector2f(0, 1)),
                new Vertex(new Vector3(Size.X, 0.0f, 0.0f), Vector3f.One, new Vector3(0, 0, 1), new Vector2f(1, 1)),
                new Vertex(new Vector3(Size.X, Size.Y, 0.0f), Vector3f.One, new Vector3(0, 0, 1), new Vector2f(1, 0)),
                new Vertex(new Vector3(0.0f, Size.Y, 0.0f), Vector3f.One, new Vector3(0, 0, 1), new Vector2f(0, 0))
            ],
            [
                2, 1, 0,
                3, 2, 0
            ]);

            Mesh.Shader = ShaderManager.Get("ui");

            UpdateAlignedPosition();

            UIManager.AddElement(this);
        }

        public virtual void Initialize() { }

        public virtual void Update()
        {
            Mesh.IsActive = IsVisible;

            UpdateAlignedPosition();
        }

        public virtual void Uninitialize() { }

        private void UpdateAlignedPosition()
        {
            Vector2f alignedPosition = ComputeAlignedPosition();

            Mesh.Transform = UITransform.Create(
                new(alignedPosition.X, alignedPosition.Y),
                Vector2f.One,
                0.0f
            );
        }

        private Vector2f ComputeAlignedPosition()
        {
            float x = Position.X;
            float y = Position.Y;

            if (Alignment.HasFlag(Alignment.Left))
                x = Position.X + AlignmentOffset.X;
            else if (Alignment.HasFlag(Alignment.Right))
                x = ScreenDimensions.X - Size.X + AlignmentOffset.X;
            else if (Alignment.HasFlag(Alignment.CenterX))
                x = (ScreenDimensions.X - Size.X) / 2 + AlignmentOffset.X;

            if (Alignment.HasFlag(Alignment.Bottom))
                y = Position.Y + AlignmentOffset.Y;
            else if (Alignment.HasFlag(Alignment.Top))
                y = ScreenDimensions.Y - Size.Y + AlignmentOffset.Y;
            else if (Alignment.HasFlag(Alignment.CenterY))
                y = (ScreenDimensions.Y - Size.Y) / 2 + AlignmentOffset.Y;

            return new Vector2f(x, y);
        }
    }
}
