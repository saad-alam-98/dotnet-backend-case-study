namespace SaudiPlus.Request
{
    public class ToggleStatusDTO<T> : IBaseEntityDTO<T>
    {
        public required T Id { get; set; }
        public required bool IsActive { get; set; }
    }
}
