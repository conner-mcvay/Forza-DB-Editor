using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forza_DB_Editor
{
    public class Engine
    {
        public int EngineID { get; set; }
        public string EngineName { get; set; }
    }

    public class Turbo
    {
        public int EngineID { get; set; }
        public int Level { get; set; }
        public int ManufacturerID { get; set; }
        public int Price { get; set; }

        public double MaxScale { get; set; }
        public double PowerMaxScale { get; set; }
        public double MinScale { get; set; }
        public double PowerMinScale { get; set; }
        public double RobScale { get; set; }
    }
}
