
namespace TeamsApp.PatientManagement.Bot.Services.MicrosoftGraph
{
    using Microsoft.Graph;
    using System;

    /// <summary>
    /// Graph Service Factory.
    /// </summary>
    public class GraphServiceFactory : IGraphServiceFactory
    {
        private readonly IGraphServiceClient serviceClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphServiceFactory"/> class.
        /// </summary>
        /// <param name="serviceClient">V1 Graph service client.</param>
        public GraphServiceFactory(
            IGraphServiceClient serviceClient)
        {
            this.serviceClient = serviceClient ?? throw new ArgumentNullException(nameof(serviceClient));
        }

        /// <inheritdoc/>
        public IUsersService GetUsersService()
        {
            return new UsersService(this.serviceClient);
        }

        public IGroupsService GetGroupsService()
        {
            return new GroupsService(this.serviceClient);
        }

        public IMessageService GetMessageService()
        {
            return new MessageService(this.serviceClient);
        }
    }
}
