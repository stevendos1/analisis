// Spendnt.Shared/DTOs/UserViewModel.cs
namespace Spendnt.Shared.DTOs
{
    public class UserViewModel
    {
        public string Id { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? ProfilePictureUrl { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }
}