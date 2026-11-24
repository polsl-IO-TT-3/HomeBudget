using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace HomeBudget.Components
{
    [ContentProperty(nameof(CardContent))]
    public partial class DashboardSectionCard : UserControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(DashboardSectionCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty HeaderBackgroundProperty =
            DependencyProperty.Register(nameof(HeaderBackground), typeof(Brush), typeof(DashboardSectionCard),
                new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D8CEF0"))));

        public static readonly DependencyProperty HeaderForegroundProperty =
            DependencyProperty.Register(nameof(HeaderForeground), typeof(Brush), typeof(DashboardSectionCard),
                new PropertyMetadata(Brushes.White));

        public static readonly DependencyProperty TotalTextProperty =
            DependencyProperty.Register(nameof(TotalText), typeof(string), typeof(DashboardSectionCard),
                new PropertyMetadata(null, OnTotalTextChanged));

        public static readonly DependencyProperty CardContentProperty =
            DependencyProperty.Register(nameof(CardContent), typeof(object), typeof(DashboardSectionCard), new PropertyMetadata(null));

        public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
        public Brush HeaderBackground { get => (Brush)GetValue(HeaderBackgroundProperty); set => SetValue(HeaderBackgroundProperty, value); }
        public Brush HeaderForeground { get => (Brush)GetValue(HeaderForegroundProperty); set => SetValue(HeaderForegroundProperty, value); }
        public string? TotalText { get => (string?)GetValue(TotalTextProperty); set => SetValue(TotalTextProperty, value); }
        public object? CardContent { get => GetValue(CardContentProperty); set => SetValue(CardContentProperty, value); }

        public Visibility FooterVisibility => string.IsNullOrEmpty(TotalText) ? Visibility.Collapsed : Visibility.Visible;

        public DashboardSectionCard()
        {
            InitializeComponent();
        }

        private static void OnTotalTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DashboardSectionCard card)
            {
                card.OnPropertyChanged(nameof(FooterVisibility));
            }
        }

        private void OnPropertyChanged(string propertyName) =>
            InvalidateVisual();
    }
}