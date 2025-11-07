using TasqueManager.Abstractions.RepositoryAbstractions;

namespace TasqueManager.Infrastructure.Repositories
{
    public class UnitOfWork: IUnitOfWork
    {
        private IAssignmentRepository _assignmentRepository;
        private DatabaseContext _context;

        public IAssignmentRepository AssignmentRepository => _assignmentRepository;

        public UnitOfWork(DatabaseContext context)
        {
            _context = context;
            _assignmentRepository = new AssignmentRepository(context);
        }

        public async Task Commit()
        {
            await _context.SaveChangesAsync();
            _context.CommitTransaction();
        }

        public void Rollback() 
        {
            _context.RollbackTransaction();
        }

        public void Dispose() 
        {
            _context.Dispose();
        }
    }
}
