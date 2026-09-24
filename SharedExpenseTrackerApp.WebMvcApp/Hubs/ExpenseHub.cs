using Microsoft.AspNetCore.SignalR;

namespace SharedExpenseTrackerApp.WebMvcApp.Hubs;

public class ExpenseHub : Hub
{
    /// <summary>
    /// Called by clients on page load to join the SignalR group for a specific expense list.
    /// Clients in the same group/list receive live unpaid-count updates.
    /// </summary>
    public async Task JoinListGroup(long groupId, long listId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, ListGroupKey(groupId, listId));
    }

    /// <summary>
    /// Called by clients when leaving a list page.
    /// </summary>
    public async Task LeaveListGroup(long groupId, long listId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, ListGroupKey(groupId, listId));
    }

    /// <summary>
    /// Called by the server (via IHubContext) after toggling an expense paid status.
    /// Pushes the new unpaid count to all members viewing the same list.
    /// </summary>
    public static string ListGroupKey(long groupId, long listId) => $"group-{groupId}-list-{listId}";
}
