using Microsoft.EntityFrameworkCore;
using TasqueManager.Abstractions.RepositoryAbstractions;
using TasqueManager.Contracts.Assignment;
using TasqueManager.Domain;


namespace TasqueManager.Infrastructure.Repositories
{
    public class AssignmentRepository: Repository<Assignment, Guid>, IAssignmentRepository
    {
        public AssignmentRepository(DatabaseContext context) : base(context)
        {
        }
        public override async Task<Assignment> GetAsync(Guid id, CancellationToken cancellationToken)
        {
            var query = Context.Set<Assignment>().AsQueryable();
            var result = await query.SingleOrDefaultAsync(c => c.Id == id, cancellationToken);
            return result!;
        }
        public async Task<List<Assignment>> GetPagedAsync(AssignmentFilterDto filterDto)
        {
            var query = GetAll()
                .Where(c => !c.Deleted);
            if (!string.IsNullOrWhiteSpace(filterDto.Title))
            {
                query = query.Where(c => c.Title == filterDto.Title);
            }
            if (filterDto.Status != AssignmentStatus.None)
            {
                query = query.Where(c => c.Status == filterDto.Status);
            }
            query = query
                .Skip((filterDto.Page - 1) * filterDto.ItemsPerPage)
                .Take(filterDto.ItemsPerPage);

            return await query.ToListAsync();
        }
    }
}
