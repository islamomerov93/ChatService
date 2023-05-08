﻿using Microsoft.AspNetCore.Identity;

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

        /// <summary>
        /// Gets or sets the last active date.
        /// </summary>
        public DateTime? LastActiveDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is online.
        /// </summary>
        public bool IsOnline { get; set; }

        public ICollection<ChatUser> Chats { get; set; }
    }
}
