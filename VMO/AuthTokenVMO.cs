namespace fuszerkomat_api.VMO
{
    public class AuthTokenVMO
    {
        public string AcessToken { get; set; }
        public DateTime AcessTokenExpires { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpires { get; set; }
        public string? PrivateKey { get; set; }
        public string? PrivateSignKey { get; set; }
        public string? PublicKey { get; set; }
    }
}
