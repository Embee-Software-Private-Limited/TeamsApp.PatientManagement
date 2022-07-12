using Newtonsoft.Json;

namespace TeamsApp.PatientManagement.Common.Models
{
    public class GroupModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
        [JsonProperty("mail")]
        public string Mail { get; set; }
    }

    public class TeamChannelModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
        [JsonProperty("membershipType")]
        public string MembershipType { get; set; }
        [JsonProperty("teamGroupId")]
        public string TeamGroupId { get; set; }
    }
}
