using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Invasion.Entity.Entities
{
    public class EntityPig() : IEntity(10.0f, 4.5f, 6.0f)
    {
        public override string RegistryName => "entity_pig";

        public override void Update()
        {
            base.Update();

            GameObject.Transform.Rotate(new(0.0f, 0.0f, 0.0f));
        }
    }
}
