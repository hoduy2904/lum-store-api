using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LumStoreAPI.Application.DTOs.CustomerDTO;

public class CustomerNoteCreateDTO
{
    // Spec field name is "content"
    [JsonPropertyName("content")]
    [Required, MaxLength(2000)]
    public string Note { get; set; } = default!;
}

public class CustomerNoteGetDTO
{
    [JsonPropertyName("id")]
    public int NoteId { get; set; }

    [JsonPropertyName("content")]
    public string Note { get; set; } = default!;

    [JsonPropertyName("createdBy")]
    public string AuthorName { get; set; } = default!;

    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; set; }
}
