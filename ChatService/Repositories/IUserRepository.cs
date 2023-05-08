namespace ChatService.Repositories
{
    using ChatService.Data.Models;

    /// <summary>
    /// The UserRepository interface.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// The get users.
        /// </summary>
        /// <returns>
        /// The <see cref="IQueryable"/>.
        /// </returns>
        IQueryable<ApplicationUser> GetUsers();

        /// <summary>
        /// The get user.
        /// </summary>
        /// <param name="condition">
        /// The condition.
        /// </param>
        /// <returns>
        /// The <see cref="ApplicationUser"/>.
        /// </returns>
        ApplicationUser GetUser(Func<ApplicationUser, bool> condition);

        /// <summary>
        /// The update user async.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task<ApplicationUser> UpdateUserAsync(ApplicationUser user);
    }
}
