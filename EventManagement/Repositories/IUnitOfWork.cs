namespace EventManagement.Repositories
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }
        void Save();
    }

}
