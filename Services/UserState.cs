namespace LogMyOJT.Services;

// Holds the "logged in" user for the current session. Nothing is saved anywhere,
// so refreshing the browser resets it. A database can replace this later.
public class UserState
{
    public bool SignedIn { get; set; }
    public string Username { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Bio { get; set; } = "";
    public string? ProfileImage { get; set; }   // data url of the picked photo

    public bool DarkMode { get; set; }
    public bool EmailNotifications { get; set; } = true;

    // the layout and top bar listen to this so they redraw when something changes
    public event Action? Changed;
    public void Notify() => Changed?.Invoke();

    public string DisplayName =>
        !string.IsNullOrWhiteSpace(FirstName) ? FirstName :
        !string.IsNullOrWhiteSpace(Username) ? Username : "Guest";

    public string WelcomeName =>
        $"{FirstName} {LastName}".Trim() is { Length: > 0 } name ? name : DisplayName;

    public string Initial =>
        !string.IsNullOrWhiteSpace(Username) ? Username[..1].ToUpper() :
        !string.IsNullOrWhiteSpace(FirstName) ? FirstName[..1].ToUpper() : "?";

    public void Clear()
    {
        SignedIn = false;
        Username = FirstName = LastName = FullName = Bio = "";
        ProfileImage = null;
        DarkMode = false;
        EmailNotifications = true;
    }
}
