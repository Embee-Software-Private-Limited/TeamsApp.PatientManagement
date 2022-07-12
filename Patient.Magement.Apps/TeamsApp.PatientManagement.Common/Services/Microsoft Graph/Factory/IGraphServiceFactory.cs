namespace TeamsApp.PatientManagement.Bot.Services.MicrosoftGraph
{
    /// <summary>
    /// Interface for Graph Service Factory.
    /// </summary>
    public interface IGraphServiceFactory
    {
        IGroupsService GetGroupsService();
        IMessageService GetMessageService();

        /// <summary>
        /// Creates an instance of <see cref="IUsersService"/> implementation.
        /// </summary>
        /// <returns>Returns an implementation of <see cref="IUsersService"/>.</returns>
        public IUsersService GetUsersService();

        
    }
}
