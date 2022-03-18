namespace TheMonolith.Data
{
    public record Score(User User, string Type, double Value)
    {
        public int Id { get; set; }
    }
}