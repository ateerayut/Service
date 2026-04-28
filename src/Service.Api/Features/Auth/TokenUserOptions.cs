namespace Service.Api.Features.Auth;

public class TokenUserOptions
{
    public const string SectionName = "TokenUser";

    public string Username { get; init; } = "demo";
    public string Password { get; init; } = "CHANGE_ME";
}
