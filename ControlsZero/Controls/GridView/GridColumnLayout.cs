namespace FunctionZero.Maui.Controls;

/// <summary>
/// Resolved column widths and offsets shared by the header and every realized row.
/// </summary>
internal sealed class GridColumnLayout
{
    private const double DefaultAutoWidth = 100.0;

    public double[] Widths { get; private set; } = Array.Empty<double>();
    public double[] Offsets { get; private set; } = Array.Empty<double>();
    public double TotalWidth { get; private set; }
    public double ViewportWidth { get; private set; }

    public void Compute(IReadOnlyList<GridColumnZero> columns, double viewportWidth, IReadOnlyList<double> autoWidths)
    {
        ViewportWidth = double.IsFinite(viewportWidth) ? Math.Max(0, viewportWidth) : 0;
        int count = columns.Count;
        if (Widths.Length != count)
        {
            Widths = new double[count];
            Offsets = new double[count];
        }

        double fixedTotal = 0;
        double starTotal = 0;

        for (int c = 0; c < count; c++)
        {
            var column = columns[c];
            if (column.IsVisible == false)
            {
                Widths[c] = 0;
                continue;
            }

            var width = column.Width;
            if (width.IsStar)
            {
                starTotal += width.Value;
                continue;
            }

            double value = width.IsAbsolute
                ? width.Value
                : (c < autoWidths.Count && autoWidths[c] > 0 ? autoWidths[c] : DefaultAutoWidth);

            Widths[c] = Clamp(value, column);
            fixedTotal += Widths[c];
        }

        double remaining = double.IsFinite(viewportWidth) && viewportWidth > 0 ? Math.Max(0, viewportWidth - fixedTotal) : 0;

        for (int c = 0; c < count; c++)
        {
            var column = columns[c];
            if (column.IsVisible && column.Width.IsStar)
                Widths[c] = Clamp(starTotal > 0 ? remaining * column.Width.Value / starTotal : 0, column);
        }

        double offset = 0;
        for (int c = 0; c < count; c++)
        {
            Offsets[c] = offset;
            offset += Widths[c];
        }
        TotalWidth = offset;
    }

    private static double Clamp(double value, GridColumnZero column)
    {
        double max = Math.Max(column.MinWidth, column.MaxWidth);
        return Math.Clamp(value, column.MinWidth, max);
    }
}
