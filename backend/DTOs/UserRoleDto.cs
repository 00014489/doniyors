namespace backend.DTOs
{
    /// <summary>A selectable account type (SuperAdmin / Admin / User).</summary>
    public class UserRoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
