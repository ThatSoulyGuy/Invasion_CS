using Invasion.Math;
using Invasion.Render;

namespace Invasion.UI
{
    public abstract class UIElement
    {
        public string Name { get; set; } = string.Empty;

        public Vector2f Position { get; set; } = Vector2f.Zero;
        public Vector2f Size { get; set; } = Vector2f.Zero;

        public UIMesh Mesh { get; set; } = null!;
        
        public bool IsVisible { get; set; } = false;

        public UIElement(string name, Vector2f position, Vector2f size)
        {
            Name = name;
            Position = position;
            Size = size;

            Mesh = UIMesh.Create(name,
            [
                new( new( -0.5f, -0.5f, 0.0f ), new( 1.0f, 1.0f, 1.0f ), new( 0.0f, 0.0f, 1.0f ), new( 0.0f, 1.0f ) ),
				new( new(  0.5f, -0.5f, 0.0f ), new( 1.0f, 1.0f, 1.0f ), new( 0.0f, 0.0f, 1.0f ), new( 1.0f, 1.0f ) ),
				new( new(  0.5f,  0.5f, 0.0f ), new( 1.0f, 1.0f, 1.0f ), new( 0.0f, 0.0f, 1.0f ), new( 1.0f, 0.0f ) ),
				new( new( -0.5f,  0.5f, 0.0f ), new( 1.0f, 1.0f, 1.0f ), new( 0.0f, 0.0f, 1.0f ), new( 0.0f, 0.0f ) )
            ],
            [
                2, 1, 0,
                3, 2, 0
            ]);

            Mesh.Shader = ShaderManager.Get("ui");

            Mesh.Transform = UITransform.Create(new(position.X, position.Y), new(size.X, size.Y), 0.0f);

            UIManager.AddElement(this);
        }

        public virtual void Initialize() { }

        public virtual void Update() { }
        
        public virtual void Uninitialize() { }
    }
}
