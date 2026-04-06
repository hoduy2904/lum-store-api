using System.ComponentModel.DataAnnotations;

namespace LumStoreAPI.Application.DTOs.CustomerDTO;

public class CustomerNoteCreateDTO
{
    [Required, MaxLength(2000)]
    public string Note { get; set; } = default!;
}

public class CustomerNoteGetDTO
{
    public int NoteId { get; set; }
    public string Note { get; set; } = default!;
    public string AuthorName { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }
}
