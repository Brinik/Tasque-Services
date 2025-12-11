using AutoMapper;
using MassTransit.NewIdProviders;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Threading;
using TasqueManager.Abstractions.RepositoryAbstractions;
using TasqueManager.Abstractions.ServiceAbstractions;
using TasqueManager.Contracts.Assignment;
using TasqueManager.Domain;
using TasqueManager.WebHost.Settings;

namespace TasqueManager.WebHost.Services
{
    public class AssignmentService: IAssignmentService
    {
        private readonly IMapper _mapper;
        private readonly IAssignmentRepository _assignmentRepository;



        public AssignmentService(
            IMapper mapper,
            IAssignmentRepository assignmentRepository)
        {
            _mapper = mapper;
            _assignmentRepository = assignmentRepository;
        }
        /// <summary>
        /// Получить задачу.
        /// </summary>
        /// <param name="id"> Идентификатор. </param>
        /// <returns> ДТО задачи. </returns>
        public async Task<AssignmentDto> GetByIdAsync(Guid id) 
        {
            var assignment = await _assignmentRepository.GetAsync(id, CancellationToken.None) ?? 
                throw new KeyNotFoundException($"Task with id: {id} not found");
            return _mapper.Map<Assignment, AssignmentDto>(assignment);
        }

        /// <summary>
        /// Создать задачу.
        /// </summary>
        /// <param name="creatingAssignmentDto"> ДТО создаваемой задачи. </param>
        public async Task<Guid> CreateAsync(CreatingAssignmentDto creatingAssignmentDto) 
        {
            var assignment = _mapper.Map<CreatingAssignmentDto, Assignment>(creatingAssignmentDto);
            var createdAssignment = await _assignmentRepository.AddAsync(assignment);
            await _assignmentRepository.SaveChangesAsync();
            return createdAssignment.Id;
        }

        /// <summary>
        /// Изменить задачу.
        /// </summary>
        /// <param name="id"> Иентификатор. </param>
        /// <param name="updatingAssignmentDto"> ДТО редактируемой задачи. </param>
        public async Task UpdateAsync(Guid id, UpdatingAssignmentDto updatingAssignmentDto) 
        {
            var assignment = await _assignmentRepository.GetAsync(id, CancellationToken.None) ?? 
                throw new KeyNotFoundException($"Task with id: {id} not found");

            assignment.Title = updatingAssignmentDto.Title;
            assignment.Description = updatingAssignmentDto.Description;
            assignment.Status = updatingAssignmentDto.Status;
            assignment.DueDate = updatingAssignmentDto.DueDate;

            _assignmentRepository.Update(assignment);
            await _assignmentRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Удалить задачу.
        /// </summary>
        /// <param name="id"> Идентификатор. </param>
        public async Task DeleteAsync(Guid id) 
        {

            var assignment = await _assignmentRepository.GetAsync(id, CancellationToken.None) ?? 
                throw new KeyNotFoundException($"Task with id: {id} not found");

            assignment.Deleted = true;
            await _assignmentRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Получить постраничный список.
        /// </summary>
        /// <param name="filterDto"> ДТО фильтра. </param>
        /// <returns> Список задач. </returns>
        public async Task<ICollection<AssignmentDto>> GetPagedAsync(AssignmentFilterDto filterDto) 
        {
            ICollection<Assignment> entities = await _assignmentRepository.GetPagedAsync(filterDto);
            return _mapper.Map<ICollection<Assignment>, ICollection<AssignmentDto>>(entities);
        }
    }
}
