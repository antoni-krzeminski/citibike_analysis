using CsvHelper.Configuration;

namespace CityBikeApp
{
    public class TripDataMap : ClassMap<TripData>
    {
        public TripDataMap()
        {
            Map(m => m.RideId).Name("ride_id");
            Map(m => m.RideableType).Name("rideable_type");
            Map(m => m.StartedAt).Name("started_at");
            Map(m => m.EndedAt).Name("ended_at");
            Map(m => m.StartStation).Name("start_station_name");
            Map(m => m.EndStation).Name("end_station_name");
            Map(m => m.MemberCasual).Name("member_casual");
        }
    }
}