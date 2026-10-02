using Microsoft.AspNetCore.Components;
using LogMyOJT.Services;

namespace LogMyOJT.Components;

// Base for every page that needs a signed in user. If nobody is signed in
// it sends them to the login page.
public abstract class SignedInPage : ComponentBase
{
    [Inject] protected UserState State { get; set; } = default!;
    [Inject] protected OjtState Ojt { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;

    protected static DateOnly Today => DateOnly.FromDateTime(DateTime.Now);

    protected override void OnInitialized()
    {
        if (!State.SignedIn) Nav.NavigateTo("/login");
    }
}
