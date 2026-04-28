using System.ComponentModel;

namespace Service.Api.Features.Auth;

public class TokenRequest
{
    [DefaultValue("demo")]
    public string Username { get; init; } = "demo";

    [DefaultValue("demo")]
    public string Password { get; init; } = "demo";
}
