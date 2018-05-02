namespace WpfIntegration.Interfaces
{
    interface IAppSettings
    {
        string Endpoint { get; }
        string ClientId { get; }
        int PhysicalRestaurantId { get; }
    }
}
