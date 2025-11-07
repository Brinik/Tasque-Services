namespace TasqueManager.Domain
{
    public interface IEntity<TId>
    {
        /// <summary>
        /// Идентификатор.
        /// </summary>
        TId Id { get; set; }
    }
}
