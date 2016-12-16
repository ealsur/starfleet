using System.Collections.Generic;

namespace starfleet.Models{
    public class GeoPoint{
        public string type { get; set; }
        public List<double> coordinates { get; set; }
    }
}