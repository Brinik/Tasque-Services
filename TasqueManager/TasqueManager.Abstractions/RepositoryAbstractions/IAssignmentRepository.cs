using TasqueManager.Domain;
using TasqueManager.Contracts.Assignment;

namespace TasqueManager.Abstractions.RepositoryAbstractions
{
    public interface IAssignmentRepository: IRepository<Assignment, Guid>
    {
        /// <summary>
        /// Получить постраничный список.
        /// </summary>
        /// <param name="filterDto"> ДТО фильтра. </param>
        /// <returns> Список курсов. </returns>
        Task<List<Assignment>> GetPagedAsync(AssignmentFilterDto filterDto);
    }
}
