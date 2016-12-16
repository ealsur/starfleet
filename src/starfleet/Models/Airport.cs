namespace starfleet.Models{
    public class Airport{
        public string id { get; set; }
        public string name { get; set; }
        public string iata { get; set; }
        public string icao { get; set; }
        public string country { get; set; }
        public string city { get; set; }
        public int altitude { get; set; }
        public int tz { get; set; }
        public GeoPoint location { get; set; }
    }
}