using TaskTracking.Web.Models;

namespace TaskTracking.Web.Models.Dto;

public class ChangeStatusRequest
{
    public Guid AuthKey { get; set; }
    public KanbanStatus NewStatus { get; set; }
}
