namespace TeamsApp.PatientManagement.Common.Providers
{
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.Extensions.Options;
    using TeamsApp.PatientManagement.Common.Models.Configuration;
    using TeamsApp.PatientManagement.Common.Models.Entities;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;

    /// <summary>
    /// Storage provider for Conversation table.
    /// </summary>
    public class DepartmentStorageProvider : IDepartmentStorageProvider
    {
        /// <summary>
        /// Table name which stores activity id of responded card.
        /// </summary>
        public const string TableName = "DepartmentEntity";


        public const string PartitionKey = "DepartmentData";
        /// <summary>
        /// Task for initialization.
        /// </summary>
        private readonly Lazy<Task> initializeTask;

        /// <summary>
        /// CloudTableClient object provides a service client for accessing the azure Table service.
        /// </summary>
        private CloudTableClient cloudTableClient;

        /// <summary>
        /// CloudTable object that represents a table.
        /// </summary>
        private CloudTable cloudTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConversationStorageProvider"/> class.
        /// </summary>
        /// <param name="optionsAccessor">A set of key/value application configuration properties.</param>
        public DepartmentStorageProvider(IOptionsMonitor<AzureStorageSettings> optionsAccessor)
        {
            this.initializeTask = new Lazy<Task>(() => this.InitializeAsync(connectionString: optionsAccessor?.CurrentValue?.StorageConnectionString));
        }


        public async Task<DepartmentEntity> GetAsync(string departmentId)
        {
            await this.EnsureInitializedAsync().ConfigureAwait(false);
            string partitionKeyCondition = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, PartitionKey);
            string rowKeyCondition = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, departmentId);
            string condition = TableQuery.CombineFilters(partitionKeyCondition, TableOperators.And, rowKeyCondition);
            TableQuery<DepartmentEntity> query = new TableQuery<DepartmentEntity>().Where(condition);
            TableContinuationToken continuationToken = null;
            var queryResult = await this.cloudTable.ExecuteQuerySegmentedAsync(query, continuationToken).ConfigureAwait(false);

            if (queryResult.Results.Count > 0)
            {
                return queryResult.Results[0];
            }
            else
            {
                return null;
            }

        }

        public async Task<IList<DepartmentEntity>> GetAllAsync()
        {
            await this.EnsureInitializedAsync().ConfigureAwait(false);
            string partitionKeyCondition = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, PartitionKey);
            //string rowKeyCondition = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, userId);
            //string condition = TableQuery.CombineFilters(partitionKeyCondition, TableOperators.And, rowKeyCondition);
            TableQuery<DepartmentEntity> query = new TableQuery<DepartmentEntity>().Where(partitionKeyCondition);

            TableContinuationToken continuationToken = null;
            List<DepartmentEntity> entities = new List<DepartmentEntity>();
            do
            {
                var queryResult = await this.cloudTable.ExecuteQuerySegmentedAsync(query, continuationToken).ConfigureAwait(false);
                entities.AddRange(queryResult.Results);
                continuationToken = queryResult.ContinuationToken;
            }
            while (continuationToken != null);

            return entities;
        }

        /// <summary>
        /// Add the activity entity object in table storage.
        /// </summary>
        /// <param name="activityEntity">Activity table entity.</param>
        /// <returns>A <see cref="Task"/> of type bool where true represents activity entity object is added in table storage successfully while false indicates failure in saving data.</returns>
        public async Task<bool> AddEntityAsync(DepartmentEntity entity)
        {

            await this.EnsureInitializedAsync().ConfigureAwait(false);
            TableOperation insertOrMergeOperation = TableOperation.InsertOrReplace(entity);
            TableResult result = await this.cloudTable.ExecuteAsync(insertOrMergeOperation).ConfigureAwait(false);
            return result.HttpStatusCode == (int)HttpStatusCode.NoContent;

        }


        public async Task<bool> DeleteEntityAsync(DepartmentEntity entity)
        {
            await this.EnsureInitializedAsync().ConfigureAwait(false);
            if (entity != null)
            {
                // An ETag property is used for optimistic concurrency during updates.
                entity.ETag = "*";
            }

            TableOperation insertOrMergeOperation = TableOperation.Delete(entity);
            TableResult result = await this.cloudTable.ExecuteAsync(insertOrMergeOperation).ConfigureAwait(false);
            return result.HttpStatusCode == (int)HttpStatusCode.NoContent;
        }

        /// <summary>
        /// Ensure table storage connection is initialized.
        /// </summary>
        /// <returns>Initialized task.</returns>
        private async Task EnsureInitializedAsync()
        {
            await this.initializeTask.Value.ConfigureAwait(false);
        }

        /// <summary>
        /// Create tables if it doesn't exist.
        /// </summary>
        /// <param name="connectionString">Storage account connection string.</param>
        /// <returns><see cref="Task"/> Representing the asynchronous operation task which represents table is created if its not existing.</returns>
        private async Task<CloudTable> InitializeAsync(string connectionString)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            this.cloudTableClient = storageAccount.CreateCloudTableClient();
            this.cloudTable = this.cloudTableClient.GetTableReference(TableName);
            if (!await this.cloudTable.ExistsAsync().ConfigureAwait(false))
            {
                await this.cloudTable.CreateIfNotExistsAsync().ConfigureAwait(false);
            }

            return this.cloudTable;
        }
    }
}
