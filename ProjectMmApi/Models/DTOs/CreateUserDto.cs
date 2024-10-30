namespace ProjectMmApi.Models.DTOs
{
    public class CreateUserDto
    {
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
