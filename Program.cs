using SominnercoreDash.Components;
using SominnercoreDash.Services;
using Supabase;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure Supabase
var supabaseUrl = builder.Configuration["Supabase:Url"] 
    ?? throw new InvalidOperationException("Supabase URL not configured");
var supabaseKey = builder.Configuration["Supabase:Key"] 
    ?? throw new InvalidOperationException("Supabase Key not configured");

// Register Supabase Client as SINGLETON (CRITICAL!)
builder.Services.AddSingleton<Supabase.Client>(_ => 
{
    var options = new SupabaseOptions
    {
        AutoRefreshToken = true,
        AutoConnectRealtime = true
    };
    
    return new Supabase.Client(supabaseUrl, supabaseKey, options);
});

// Register Authentication Service
builder.Services.AddScoped<SupabaseAuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// Redirect root "/" to "/signin" at HTTP level
app.MapGet("/", () => Results.Redirect("/signin"));

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();