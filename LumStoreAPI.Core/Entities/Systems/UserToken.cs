namespace LumStoreAPI.Core.Entities.Systems
{
    public class UserToken
    {
        public int UserID { get; set; }
        public Guid TokenID { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime ValidTo { get; set; }

        public virtual User User { get; set; } = default!;
    }
}
