using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forza_DB_Editor
{
    public class EngineSwap
    {
        public int UpgradeEngineID { get; set; }
        public int CarID { get; set; }
        public string CarName { get; set; }
        public int EngineID { get; set; }
        public string EngineName { get; set; }
        public int Level { get; set; }
        public bool IsStock { get; set; }
        public int Price { get; set; }
    }
}
