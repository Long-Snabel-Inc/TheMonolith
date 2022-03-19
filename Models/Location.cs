namespace TheMonolith.Models;

public class Location
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Accuracy { get; set; }
    
    public override string ToString() => $"Location: ({Latitude}, {Longitude}) with accuracy {Accuracy}";
}

internal static class LocationExtensions
{
    public static double DistanceTo(this Location baseCoordinates, Location targetCoordinates)
    {
        var baseRad = Math.PI * baseCoordinates.Latitude / 180;
        var targetRad = Math.PI * targetCoordinates.Latitude/ 180;
        var theta = baseCoordinates.Longitude - targetCoordinates.Longitude;
        var thetaRad = Math.PI * theta / 180;

        var dist =
            Math.Sin(baseRad) * Math.Sin(targetRad) + Math.Cos(baseRad) *
            Math.Cos(targetRad) * Math.Cos(thetaRad);
        dist = Math.Acos(dist);

        dist = dist * 180 / Math.PI;
        dist = dist * 60 * 1.1515;
        return dist * 1609.344;
    }
}