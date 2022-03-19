namespace TheMonolith.Data
{
    public record Score(User User, string Type, double Value)
    {
        public const string GoogleType = "GOOGLETEXT";
        public const string LocationType = "LOCATION";
        
        public int Id { get; set; }
    }
}