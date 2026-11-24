using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HomeBudget.Components
{
    public partial class KpiCard : UserControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(KpiCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ValueTextProperty =
            DependencyProperty.Register(nameof(ValueText), typeof(string), typeof(KpiCard), new PropertyMetadata("0,00 zł"));

        public static readonly DependencyProperty ValueBrushProperty =
            DependencyProperty.Register(nameof(ValueBrush), typeof(Brush), typeof(KpiCard),
                new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B"))));

        public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
        public string ValueText { get => (string)GetValue(ValueTextProperty); set => SetValue(ValueTextProperty, value); }
        public Brush ValueBrush { get => (Brush)GetValue(ValueBrushProperty); set => SetValue(ValueBrushProperty, value); }

        public KpiCard()
        {
            InitializeComponent();
        }
    }
}