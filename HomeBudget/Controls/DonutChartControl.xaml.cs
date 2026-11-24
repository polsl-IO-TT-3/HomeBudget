using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HomeBudget.Controls;

public partial class DonutChartControl : UserControl
{
    public static readonly DependencyProperty SpentRatioProperty =
        DependencyProperty.Register(
            nameof(SpentRatio),
            typeof(double),
            typeof(DonutChartControl),
            new PropertyMetadata(0.0, OnSpentRatioChanged));

    public double SpentRatio
    {
        get => (double)GetValue(SpentRatioProperty);
        set => SetValue(SpentRatioProperty, value);
    }

    public DonutChartControl()
    {
        InitializeComponent();
        UpdateChart();
    }

    private static void OnSpentRatioChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DonutChartControl control)
            control.UpdateChart();
    }

    private void UpdateChart()
    {
        double clamped = Math.Max(0.0, Math.Min(1.0, SpentRatio));
        CenterPercentText.Text = $"{Math.Round(SpentRatio * 100)}%";

        if (clamped <= 0.001)
        {
            SpentArcPath.Data = Geometry.Empty;
            return;
        }

        // Unikamy pełnego 360 stopni dla pojedynczego ArcSegment (359.99 daje pełne koło)
        double angle = clamped >= 0.999 ? 359.99 : clamped * 360.0;
        double radius = 72.0; // (180 - 36) / 2
        double cx = 100.0;
        double cy = 100.0;

        // Start na godzinie 12:00 (-90 deg)
        double startRad = -Math.PI / 2.0;
        double endRad = startRad + angle * Math.PI / 180.0;

        var startPoint = new Point(cx + radius * Math.Cos(startRad), cy + radius * Math.Sin(startRad));
        var endPoint = new Point(cx + radius * Math.Cos(endRad), cy + radius * Math.Sin(endRad));

        bool isLargeArc = angle > 180.0;

        var figure = new PathFigure { StartPoint = startPoint, IsClosed = false };
        figure.Segments.Add(new ArcSegment
        {
            Point = endPoint,
            Size = new Size(radius, radius),
            IsLargeArc = isLargeArc,
            SweepDirection = SweepDirection.Clockwise
        });

        var geometry = new PathGeometry();
        geometry.Figures.Add(figure);
        SpentArcPath.Data = geometry;
        SpentArcPath.Stroke = SpentRatio > 1.0
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"))
            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A78BFA"));
    }
}
