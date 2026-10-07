namespace Core.Domain;

public static class OrderPolicy
{
    public static void AddProductToOrder(
        Order order,
        Product product,
        int quantity)
    {
        if (order is null)
            throw new ArgumentNullException(nameof(order));

        if (product is null)
            throw new ArgumentNullException(nameof(product));

        if (!product.IsAvailable)
            throw new InvalidOperationException(
                $"Товар {product.Id} \"{product.Name}\" " +
                "зараз недоступний для замовлення");

        order.AddLine(
            product.Id,
            product.Name,
            product.Price,
            quantity);
    }
}