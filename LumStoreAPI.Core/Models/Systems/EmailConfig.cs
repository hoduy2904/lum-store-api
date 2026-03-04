namespace LumStoreAPI.Infrastructure.Models
{
    public record class EmailConfig(string host, int port, string username, string password);
}
