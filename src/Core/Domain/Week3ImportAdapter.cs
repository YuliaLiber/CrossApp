using Core.Dto;

namespace Core.Domain;

public static class Week3ImportAdapter
{
    public static ImportResult<OrderLine> ToOrderLines(
        ImportResult<ProductDto> importResult)
    {
        if (importResult is null)
            throw new ArgumentNullException(
                nameof(importResult));

        var items = new List<OrderLine>();

        var errors = new List<string>(
            importResult.Errors);

        for (int i = 0; i < importResult.Items.Count; i++)
        {
            ProductDto dto =
                importResult.Items[i];

            try
            {
                OrderLine line =
                    OrderLine.Create(
                        dto.Id,
                        dto.Name,
                        dto.Price,
                        1);

                items.Add(line);
            }
            catch (ArgumentException ex)
            {
                errors.Add(
                    $"елемент {i + 1}: {ex.Message}");
            }
        }

        return new ImportResult<OrderLine>(
            items,
            errors);
    }
}