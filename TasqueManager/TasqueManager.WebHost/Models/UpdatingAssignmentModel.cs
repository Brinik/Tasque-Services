using System.Text.Json.Serialization;
using TasqueManager.Domain;

namespace TasqueManager.WebHost.Models
{
    public class UpdatingAssignmentModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AssignmentStatus Status { get; set; }
    }
}
