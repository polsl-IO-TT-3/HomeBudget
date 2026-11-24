using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HomeBudget.Components
{
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
            if (d is DonutChartControl chart) chart.UpdateChart();
        }

        private void UpdateChart()
        {
            double ratio = Math.Max(0.0, SpentRatio);
            PercentText.Text = $"{ratio * 100:0}%";
            SpentArc.Stroke = ratio > 1.0
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DC2626"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7C5CBF"));

            if (ratio <= 0.001)
            {
                SpentArc.Data = null;
                return;
            }

            double clamped = Math.Min(ratio, 0.9999);
            double angle = clamped * 360.0;

            const double center = 78.0;
            const double radius = 68.0;

            double startRad = -Math.PI / 2.0;
            double endRad = startRad + (angle * Math.PI / 180.0);

            Point startPoint = new(center + radius * Math.Cos(startRad), center + radius * Math.Sin(startRad));
            Point endPoint = new(center + radius * Math.Cos(endRad), center + radius * Math.Sin(endRad));

            var figure = new PathFigure { StartPoint = startPoint, IsClosed = false };
            figure.Segments.Add(new ArcSegment
            {
                Point = endPoint,
                Size = new Size(radius, radius),
                IsLargeArc = angle > 180.0,
                SweepDirection = SweepDirection.Clockwise
            });

            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            SpentArc.Data = geometry;
        }
    }
}