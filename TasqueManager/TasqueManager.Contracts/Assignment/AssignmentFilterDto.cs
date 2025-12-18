using System.Text.Json.Serialization;
using TasqueManager.Domain;

namespace TasqueManager.Contracts.Assignment
{
    public class AssignmentFilterDto
    {
        public string? Title { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AssignmentStatus Status { get; set; }
        public int ItemsPerPage { get; set; }

        public int Page { get; set; }
    }
}
