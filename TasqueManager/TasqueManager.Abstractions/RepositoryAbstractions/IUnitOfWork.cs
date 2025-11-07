namespace TasqueManager.Abstractions.RepositoryAbstractions
{
    public interface IUnitOfWork : IDisposable
    {
        IAssignmentRepository AssignmentRepository { get; }
        Task Commit();
        void Rollback();
    }
}
