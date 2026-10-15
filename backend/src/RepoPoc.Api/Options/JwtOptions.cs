namespace RepoPoc.Api.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "RepoPoc.Api";
    public string Audience { get; set; } = "RepoPoc.Web";
    public string Key { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
}
