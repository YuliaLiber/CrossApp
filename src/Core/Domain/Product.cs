namespace Core.Domain;

public sealed class Product
{
    public string Id { get; }
    public string Name { get; }
    public decimal Price { get; }
    public bool IsAvailable { get; }

    private Product(
        string id,
        string name,
        decimal price,
        bool isAvailable)
    {
        Id = id;
        Name = name;
        Price = price;
        IsAvailable = isAvailable;
    }

    public static Product Create(
        string id,
        string name,
        decimal price,
        bool isAvailable)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException(
                "Ідентифікатор товару не може бути порожнім",
                nameof(id));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Назва товару не може бути порожньою",
                nameof(name));

        if (price < 0)
            throw new ArgumentOutOfRangeException(
                nameof(price),
                price,
                "Ціна товару не може бути від'ємною");

        return new Product(
            id.Trim(),
            name.Trim(),
            price,
            isAvailable);
    }
}