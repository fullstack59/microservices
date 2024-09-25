namespace Play.Inventory.Service.Dtos
{
    public record GrantItemsDto(Guid userId, Guid CatalogItemId, int Quantity);
    public record InventoryItemDto(Guid catalogItemId, string Name, string Description, int Quantity, DateTimeOffset AcquireDate);
    public record CatalogItemDto(Guid Id, string Name, string Description);
}