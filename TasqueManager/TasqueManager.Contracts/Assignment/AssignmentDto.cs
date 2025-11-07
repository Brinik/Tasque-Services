using System.Text.Json.Serialization;
using TasqueManager.Domain;

namespace TasqueManager.Contracts.Assignment
{
    public class AssignmentDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AssignmentStatus Status { get; set; }
    }
}
