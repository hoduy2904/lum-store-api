namespace LumStoreAPI.Core.Models.Systems
{
    public class EmailAttachment
    {
        public string FileName { get; set; }
        public Stream Stream { get; set; }

        public EmailAttachment(string fileName, Stream stream)
        {
            this.FileName = fileName;
            this.Stream = stream;
        }
    }
}
