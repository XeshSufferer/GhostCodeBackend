namespace GhostCodeBackend.AppHost;

public static class Extensions
{
    public static TBuilder WithJwtAuthSettings<TBuilder>(this TBuilder builder) where TBuilder : IResourceBuilder<ContainerResource>
    {
        builder
            .WithEnvironment("JWTAudience", "Audience")
            .WithEnvironment("JWTIssuer", "Issuer")
            .WithEnvironment("JWTKey", "SUPER_SECRET_256_BIT_KEY_AT_LEAST_32_CHARS")
            .WithEnvironment("JWTExpireMinutes", "15")
            .WithEnvironment("RefreshExpireDays", "30");
        return builder;
    }
    
    public static TBuilder WithPort<TBuilder>(this TBuilder builder, int port) where TBuilder : IResourceBuilder<ContainerResource>
    {
        if(port > 65535 || port < 0) throw new ArgumentOutOfRangeException(nameof(port));
        builder.WithEnvironment("ASPNETCORE_HTTP_PORTS", port.ToString());
        return builder;
    }
}