namespace SaudiPlus.Request
{
    public interface IBaseEntityDTO<T>
    {
        T Id { get; set; }
    }

    public class BaseEntityDTO<T> : IBaseEntityDTO<T>
    {
        public virtual required T Id { get; set; }
    }
}
