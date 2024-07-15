namespace JWT_Authentication.Models
{
    public class RevokeTokenModel
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime RevokedAt { get; set; }

    }
}
