namespace TheMonolith.Data
{
    public record User(string UserName, string Email)
    {
        public int Id { get; set; }
        public string Password { get; set; }
    }
}