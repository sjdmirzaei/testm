using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tsolesetting
{
    public class ConfigParams_FormSetting
    {
        public Color MeshColor = Color.Yellow;
        public Color LineColor = Color.Black;
        public int LineWeight = 5;
    }

    public enum StructureTypes { None, Arrows, Tetra }
}
