using System.ComponentModel.DataAnnotations;

namespace LumStoreAPI.Application.DTOs.OrderDTO;

public class OrderNoteCreateDTO
{
    [Required, MaxLength(2000)]
    public string Note { get; set; } = default!;
}

public class OrderNoteGetDTO
{
    public int NoteId { get; set; }
    public string Note { get; set; } = default!;
    public string AuthorName { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }
}
