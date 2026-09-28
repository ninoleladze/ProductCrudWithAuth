namespace HmCrudPLusAuth.Common
{
    public class Entity
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
