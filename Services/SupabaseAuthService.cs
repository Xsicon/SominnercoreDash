using Supabase;
using Supabase.Gotrue;

namespace SominnercoreDash.Services;

public class SupabaseAuthService
{
    private readonly Supabase.Client _supabase;
    private readonly ILogger<SupabaseAuthService> _logger;

    public SupabaseAuthService(Supabase.Client supabase, ILogger<SupabaseAuthService> logger)
    {
        _supabase = supabase;
        _logger = logger;
    }

    public async Task<AuthResponse> SignInAsync(string email, string password)
    {
        try
        {
            _logger.LogInformation("Attempting sign in for: {Email}", email);
            
            var session = await _supabase.Auth.SignIn(email, password);

            if (session?.User != null)
            {
                _logger.LogInformation(" User signed in successfully: {Email}", email);
                
                return new AuthResponse
                {
                    Success = true,
                    User = session.User,
                    Session = session
                };
            }

            _logger.LogWarning("Sign-in returned no user");
            return new AuthResponse
            {
                Success = false,
                ErrorMessage = "Invalid email or password"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during sign in");
            return new AuthResponse
            {
                Success = false,
                ErrorMessage = ex.Message ?? "An error occurred during sign in"
            };
        }
    }

    public async Task SignOutAsync()
    {
        try
        {
            await _supabase.Auth.SignOut();
            _logger.LogInformation("User signed out successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during sign out");
        }
    }

    public User? GetCurrentUser()
    {
        var user = _supabase.Auth.CurrentUser;
        
        if (user != null)
        {
            _logger.LogInformation(" Current user: {Email}", user.Email);
        }
        else
        {
            _logger.LogWarning(" No current user in session");
        }
        
        return user;
    }

    public Session? GetCurrentSession()
    {
        return _supabase.Auth.CurrentSession;
    }

    public bool IsAuthenticated()
    {
        return _supabase.Auth.CurrentUser != null;
    }
}

public class AuthResponse
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public User? User { get; set; }
    public Session? Session { get; set; }
}