namespace ChatService.Repositories
{
    using System;

    using ChatService.Data;
    using ChatService.Data.Models;

    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRepository"/> class.
        /// </summary>
        /// <param name="dbContext">
        /// The db context.
        /// </param>
        public UserRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Get users.
        /// </summary>
        /// <returns>
        /// The <see cref="IQueryable"/>.
        /// </returns>
        public IQueryable<ApplicationUser> GetUsers()
        {
            return this._dbContext.Users;
        }

        /// <summary>
        /// The get user.
        /// </summary>
        /// <param name="condition">
        /// The condition.
        /// </param>
        /// <returns>
        /// The <see cref="ApplicationUser"/>.
        /// </returns>
        public ApplicationUser GetUser(Func<ApplicationUser, bool> condition)
        {
            return _dbContext.Users.FirstOrDefault(condition);
        }

        /// <summary>
        /// Update user async.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<ApplicationUser> UpdateUserAsync(ApplicationUser user)
        {
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
            return user;
        }
    }
}