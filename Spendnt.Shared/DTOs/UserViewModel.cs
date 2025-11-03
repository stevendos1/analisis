// Spendnt.Shared/DTOs/UserViewModel.cs
namespace Spendnt.Shared.DTOs
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }
}