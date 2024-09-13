using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Invasion.ECS
{
    public abstract class Component
    {
        public GameObject GameObject { get; internal set; } = null!;

        public virtual void Initialize() { }

        public virtual void Update() { }
        public virtual void Render() { }

        public virtual void CleanUp() { }
    }
}
