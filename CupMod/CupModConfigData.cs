using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CupMod
{
    public class CupModConfigData
    {
        public bool throwingEnabled = true;
        public Dictionary<String, float> breakChances =
            new Dictionary<string, float> { 
                ["tankard"] = 0.0f, 
                ["claycup"] = 0.2f,
                ["claymug"] = 0.2f,
                ["clayshot"] = 0.2f,
                ["wineglass"] = 0.85f,
            };
    }
}
