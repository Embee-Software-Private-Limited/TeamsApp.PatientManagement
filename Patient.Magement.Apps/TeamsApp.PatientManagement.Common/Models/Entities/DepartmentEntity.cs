// <copyright file="ActivityEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using System;

namespace TeamsApp.PatientManagement.Common.Models.Entities
{

    /// <summary>
    /// Activity entity to store activity id and guid for mapping purpose.
    /// </summary>
    public class DepartmentEntity : TableEntity
    {
        /// <summary>
        /// Activity table store partition key name.
        /// </summary>
        public const string DepartmentPartitionKey = "DepartmentData";

        /// <summary>
        /// Initializes a new instance of the <see cref="DepartmentEntity"/> class.
        /// </summary>
        public DepartmentEntity()
        {
            this.PartitionKey = DepartmentPartitionKey;
        }

        /// <summary>
        /// Gets or sets the activity reference id.
        /// </summary>
        /// 
        [JsonProperty("departmentId")]
        public string DepartmentId
        {
            get { return this.RowKey; }
            set { this.RowKey = value; }
        }
        [JsonProperty("departmentName")]
        public string DepartmentName { get; set; }

        [JsonProperty("channelId")]
        public string ChannelId { get; set; }
        [JsonProperty("channelName")]
        public string ChannelName { get; set; }

        [JsonProperty("teamId")]
        public string TeamId { get; set; }

        [JsonProperty("teamName")]
        public string TeamName { get; set; }
    }
}
