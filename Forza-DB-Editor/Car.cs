using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forza_DB_Editor
{
    public class Car
    {
        public int Id { get; set; }
        public int Year { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string FullName { get; set; }
        public int FrontWheelDiameterIN { get; set; }
        public int FrontTireAspect { get; set; }
        public double ModelFrontTrackOuter { get; set; }
        public int RearWheelDiameterIN { get; set; }
        public int RearTireAspect { get; set; }
       
        public double ModelRearTrackOuter { get; set; }
    }
}
