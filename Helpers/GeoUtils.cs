namespace fuszerkomat_api.Helpers
{
    public class GeoUtils
    {
        private const double EarthRadiusKm = 6371.0;

        public static double DistanceKm(double lat1, double lon1, double lat2, double lon2)
        {
            double dLat = DegreesToRadians(lat2 - lat1);
            double dLon = DegreesToRadians(lon2 - lon1);

            lat1 = DegreesToRadians(lat1);
            lat2 = DegreesToRadians(lat2);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1) * Math.Cos(lat2) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return EarthRadiusKm * c;
        }

        public static double GetDistanceBetween(double lat1, double lon1, double lat2, double lon2)
            => DistanceKm(lat1, lon1, lat2, lon2);

        private static double DegreesToRadians(double deg) => deg * Math.PI / 180.0;
    }
}
