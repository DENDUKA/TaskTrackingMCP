namespace TaskTracking.Web.Models.Dto;

public class AddCommentRequest
{
    public Guid AuthKey { get; set; }
    public string Text { get; set; } = string.Empty;
}
