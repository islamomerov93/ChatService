using Microsoft.AspNetCore.Identity;

namespace ChatService.Data.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public ApplicationUser() : base()
        {
            Chats = new List<ChatUser>();
        }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        public string LastName { get; set; }

        public ICollection<ChatUser> Chats { get; set; }
    }
}
