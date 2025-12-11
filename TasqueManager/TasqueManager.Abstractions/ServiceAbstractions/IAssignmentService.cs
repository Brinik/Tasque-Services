using TasqueManager.Contracts.Assignment;

namespace TasqueManager.Abstractions.ServiceAbstractions
{
    public interface IAssignmentService
    {
        /// <summary>
        /// Получить задачу.
        /// </summary>
        /// <param name="id"> Идентификатор. </param>
        /// <returns> ДТО задачи. </returns>
        Task<AssignmentDto> GetByIdAsync(Guid id);

        /// <summary>
        /// Создать задачу.
        /// </summary>
        /// <param name="creatingCourseDto"> ДТО создаваемой задачи. </param>
        Task<Guid> CreateAsync(CreatingAssignmentDto creatingAssignmentDto);

        /// <summary>
        /// Изменить задачу.
        /// </summary>
        /// <param name="id"> Иентификатор. </param>
        /// <param name="updatingCourseDto"> ДТО редактируемой задачи. </param>
        Task UpdateAsync(Guid id, UpdatingAssignmentDto updatingAssignmentDto);

        /// <summary>
        /// Удалить задачу.
        /// </summary>
        /// <param name="id"> Идентификатор. </param>
        Task DeleteAsync(Guid id);

        /// <summary>
        /// Получить постраничный список.
        /// </summary>
        /// <param name="filterDto"> ДТО фильтра. </param>
        /// <returns> Список задач. </returns>
        Task<ICollection<AssignmentDto>> GetPagedAsync(AssignmentFilterDto filterDto);
    }
}
