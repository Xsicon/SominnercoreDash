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
                _logger.LogInformation("User signed in successfully: {Email}", email);
                
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
                ErrorMessage = "Invalid email or password. Please check your credentials and try again."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during sign in");
            
            // Provide user-friendly error messages based on exception type
            var errorMessage = ex.Message?.ToLower() switch
            {
                var msg when msg.Contains("invalid login credentials") => 
                    "Invalid email or password. Please check your credentials and try again.",
                var msg when msg.Contains("email not confirmed") => 
                    "Please verify your email address before signing in. Check your inbox for a verification link.",
                var msg when msg.Contains("network") || msg.Contains("connection") => 
                    "Unable to connect to the server. Please check your internet connection and try again.",
                var msg when msg.Contains("too many requests") => 
                    "Too many sign-in attempts. Please wait a few minutes before trying again.",
                _ => "An error occurred during sign in. Please try again or contact support if the problem persists."
            };

            return new AuthResponse
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }

    public async Task<AuthResponse> SignUpAsync(string email, string password, string fullName, string accountType)
    {
        try
        {
            _logger.LogInformation("Attempting sign up for: {Email}", email);

            // Create the user account
            var session = await _supabase.Auth.SignUp(email, password, new SignUpOptions
            {
                Data = new Dictionary<string, object>
                {
                    { "full_name", fullName },
                    { "role", accountType.ToLower() }
                }
            });

            if (session?.User != null)
            {
                _logger.LogInformation("User signed up successfully: {Email}", email);
                
                return new AuthResponse
                {
                    Success = true,
                    User = session.User,
                    Session = session,
                    Message = "Account created successfully! Please check your email to verify your account."
                };
            }

            _logger.LogWarning("Sign-up returned no user");
            return new AuthResponse
            {
                Success = false,
                ErrorMessage = "Failed to create account. Please try again."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during sign up");
            
            // Provide user-friendly error messages based on exception type
            var errorMessage = ex.Message?.ToLower() switch
            {
                var msg when msg.Contains("user already registered") || msg.Contains("already exists") => 
                    "An account with this email already exists. Please sign in or use a different email address.",
                var msg when msg.Contains("invalid email") => 
                    "Please enter a valid email address.",
                var msg when msg.Contains("password") && msg.Contains("weak") => 
                    "Password is too weak. Please use a stronger password with at least 6 characters.",
                var msg when msg.Contains("network") || msg.Contains("connection") => 
                    "Unable to connect to the server. Please check your internet connection and try again.",
                var msg when msg.Contains("too many requests") => 
                    "Too many sign-up attempts. Please wait a few minutes before trying again.",
                _ => "An error occurred during sign up. Please try again or contact support if the problem persists."
            };

            return new AuthResponse
            {
                Success = false,
                ErrorMessage = errorMessage
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
            _logger.LogInformation("Current user: {Email}", user.Email);
        }
        else
        {
            _logger.LogWarning("No current user in session");
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
    public string? Message { get; set; }
    public User? User { get; set; }
    public Session? Session { get; set; }
}