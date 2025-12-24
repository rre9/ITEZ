namespace ITHelpDesk.ViewModels;

public record AdminConfirmModalViewModel(
    string Id,
    string Title,
    string Message,
    string? ActionUrl,
    string UserId);

