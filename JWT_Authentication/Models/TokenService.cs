using JWT_Authentication.Data;

namespace JWT_Authentication.Models
{
    public class TokenService
    {
        private readonly ApplicationDbContext _db;

        public TokenService(ApplicationDbContext db)
        {
                _db = db;
        }

        public string RevokeToken(string token)
        {
            if(_db.RevokeTokens.Any(rwk => rwk.Token == token))
            {
                return $"Token {token} is already revoked";
            }
            var revokedToken = new RevokeTokenModel();
            revokedToken.Token = token;
            revokedToken.RevokedAt = DateTime.UtcNow;
            _db.RevokeTokens.Add(revokedToken);
            _db.SaveChanges();

            return "Token has been revoked" ;
        }

        public bool IsTokenRevoked(string token)
        {
            return _db.RevokeTokens.Any(r => r.Token == token);   
        }

    }
}
