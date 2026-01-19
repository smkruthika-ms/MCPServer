namespace MCPServer.Services;

/// <summary>
/// Scoped service to hold the current request's authentication token
/// </summary>
public class AuthTokenContext
{
    private string? _token;
    
    public string? Token 
    { 
        get => _token;
        set 
        { 
            _token = value;
            Console.WriteLine($"[TOKEN CONTEXT] Token set: {(!string.IsNullOrEmpty(value) ? "YES ✓" : "NO ✗")}");
        }
    }
}
