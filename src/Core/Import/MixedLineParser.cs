using System.Globalization;
using Core.Dto;

namespace Core.Import;

public abstract record MixedRecord;

public sealed record ProductMixed(ProductDto Value)
    : MixedRecord;

public sealed record WarehouseMixed(WarehouseDto Value)
    : MixedRecord;

public static class MixedLineParser
{
    public static MixedRecord? Parse(string line)
    {
        string[] parts = line.Split(
            ';',
            StringSplitOptions.TrimEntries);

        return parts switch
        {
            ["P", var id, var name, var priceText]
                when decimal.TryParse(
                    priceText,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out decimal price) && price >= 0
                => new ProductMixed(
                    new ProductDto(id, name, price)),

            ["W", var id, var name]
                => new WarehouseMixed(
                    new WarehouseDto(id, name)),

            _ => null
        };
    }
}