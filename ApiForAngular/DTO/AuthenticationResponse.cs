namespace ApiForAngular.DTO
{
    public class AuthenticationResponse
    {
        public Guid Id { get; set; }
        public string? PersonName { set; get; }
        public string? Token { set; get; }
        public string? Email { set; get; }
        public DateTime Expire { set; get; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}
