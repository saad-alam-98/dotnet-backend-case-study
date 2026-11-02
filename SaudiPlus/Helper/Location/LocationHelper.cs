namespace SaudiPlus.Helper.Location
{
    public class LocationHelper
    {
        public static double CalculateDistance(
            (double lat, double lon) Origin, 
            (double lat, double lon) Destination
        )
        {
            const double R = 6371;
            double latRad1 = DegreesToRadians(Origin.lat);
            double latRad2 = DegreesToRadians(Destination.lat);
            double deltaLat = DegreesToRadians(Destination.lat - Origin.lat);
            double deltaLon = DegreesToRadians(Destination.lon - Origin.lon);

            double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                       Math.Cos(latRad1) * Math.Cos(latRad2) *
                       Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = R * c;
            return Math.Round(distance, 2);
        }

        public static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
}
