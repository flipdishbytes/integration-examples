namespace WpfIntegration.Interfaces
{
    interface IAppSettings
    {
        string Endpoint { get; }
        string OAuthClientId { get; }
        string OAuthSecretKey { get; }
        int PhysicalRestaurantId { get; }
    }
}
