namespace TheMonolith.Models;

public class Location
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal Accuracy { get; set; }
    
    public override string ToString() => $"Location: ({Latitude}, {Longitude}) with accuracy {Accuracy}";
}

public static class LocationExtensions
{
    public static decimal DistanceTo(this Location baseCoordinates, Location targetCoordinates)
    {
        var baseRad = Math.PI * (double)baseCoordinates.Latitude / 180;
        var targetRad = Math.PI * (double)targetCoordinates.Latitude/ 180;
        var theta = baseCoordinates.Longitude - targetCoordinates.Longitude;
        var thetaRad = Math.PI * (double)theta / 180;

        double dist =
            Math.Sin(baseRad) * Math.Sin(targetRad) + Math.Cos(baseRad) *
            Math.Cos(targetRad) * Math.Cos(thetaRad);
        dist = Math.Acos(dist);

        dist = dist * 180 / Math.PI;
        dist = dist * 60 * 1.1515;
        return (decimal)(dist * 1.609344); // Kilometer
    }
}