using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Server.Corvax.Elzuosa
{
    [RegisterComponent]
    public sealed partial class ElzuosaColorComponent : Component
    {
        public Color SkinColor { get; set; }
    }
}
