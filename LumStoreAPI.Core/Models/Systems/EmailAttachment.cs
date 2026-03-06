namespace LumStoreAPI.Core.Models.Systems
{
    public class EmailAttachment
    {
        public Guid FileID { get; set; }
        public string FileName { get; set; }
        public Stream Stream { get; set; }

        public EmailAttachment(string fileName, Stream stream, Guid fileID)
        {
            this.FileName = fileName;
            this.Stream = stream;
            this.FileID = fileID;
        }
    }
}
