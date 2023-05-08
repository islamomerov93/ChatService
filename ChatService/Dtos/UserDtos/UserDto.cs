namespace ChatService.Dtos.UserDtos
{
    /// <summary>
    /// The user dto.
    /// </summary>
    public class UserDto 
    {
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        public Guid UserId { get; set; }

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
    }
}
