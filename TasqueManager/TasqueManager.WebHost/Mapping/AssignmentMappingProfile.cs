using AutoMapper;
using TasqueManager.Domain;
using TasqueManager.Contracts.Assignment;
using TasqueManager.WebHost.Models;

namespace TasqueManager.WebHost.Mapping
{
    public class AssignmentMappingProfile: Profile
    {
        public AssignmentMappingProfile()
        {
            CreateMap<AssignmentDto, AssignmentModel>();
            CreateMap<CreatingAssignmentModel, CreatingAssignmentDto>();
            CreateMap<UpdatingAssignmentModel, UpdatingAssignmentDto>();
            CreateMap<AssignmentFilterModel, AssignmentFilterDto>();

            CreateMap<Assignment, AssignmentDto>();
            CreateMap<AssignmentDto, Assignment>()
                .ForMember(d => d.Deleted, map => map.Ignore());

            CreateMap<CreatingAssignmentDto, Assignment>()
                .ForMember(d => d.Id, map => map.Ignore())
                .ForMember(d => d.Deleted, map => map.Ignore())
                .ForMember(d => d.Status, map => map.Ignore());

            CreateMap<UpdatingAssignmentDto, Assignment>()
                    .ForMember(d => d.Id, map => map.Ignore())
                    .ForMember(d => d.Deleted, map => map.Ignore());
        }
    }
}
