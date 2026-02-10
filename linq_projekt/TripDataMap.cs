using System;

namespace CityBikeApp
{
    public class TripData
    {
       public string? RideId { get; set; }
        public string? RideableType { get; set; } 
        public DateTime StartedAt { get; set; }
        public DateTime EndedAt { get; set; }
        public string? StartStation { get; set; }
        public string? EndStation { get; set; }
        public string? MemberCasual { get; set; }
        
        public double DurationInMinutes => (EndedAt - StartedAt).TotalMinutes;
    }
}