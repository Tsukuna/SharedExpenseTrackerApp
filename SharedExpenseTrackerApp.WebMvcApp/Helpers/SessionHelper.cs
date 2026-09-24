namespace SharedExpenseTrackerApp.WebMvcApp.Helpers;

public static class SessionHelper
{
    private const string UserIdKey = "UserId";
    private const string FullNameKey = "FullName";
    private const string PhoneNumberKey = "PhoneNumber";

    // ─── Setters ──────────────────────────────────────────────────────────────
    public static void SetUserId(this ISession session, long userId)
        => session.SetString(UserIdKey, userId.ToString());

    public static void SetFullName(this ISession session, string fullName)
        => session.SetString(FullNameKey, fullName);

    public static void SetPhoneNumber(this ISession session, string phone)
        => session.SetString(PhoneNumberKey, phone);

    // ─── Getters ──────────────────────────────────────────────────────────────
    public static long? GetUserId(this ISession session)
    {
        var val = session.GetString(UserIdKey);
        return long.TryParse(val, out var id) ? id : null;
    }

    public static string? GetFullName(this ISession session)
        => session.GetString(FullNameKey);

    public static string? GetPhoneNumber(this ISession session)
        => session.GetString(PhoneNumberKey);

    // ─── Helpers ──────────────────────────────────────────────────────────────
    public static bool IsAuthenticated(this ISession session)
        => session.GetUserId().HasValue;

    public static void ClearUser(this ISession session)
    {
        session.Remove(UserIdKey);
        session.Remove(FullNameKey);
        session.Remove(PhoneNumberKey);
    }
}
