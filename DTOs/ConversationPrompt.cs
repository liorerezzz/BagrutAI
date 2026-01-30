namespace Prog3_WebApi_Javascript.DTOs;

public class ConversationPrompt
{
    public string Question { get; set; }
    public string? PreviousResponseID { get; set; }
    public string? FileId { get; set; }
}