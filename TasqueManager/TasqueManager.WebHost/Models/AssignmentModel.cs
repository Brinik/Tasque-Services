using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;
using TasqueManager.Domain;
using System.ComponentModel.DataAnnotations;

namespace TasqueManager.WebHost.Models
{
    public class AssignmentModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AssignmentStatus Status { get; set; }
    }
}
