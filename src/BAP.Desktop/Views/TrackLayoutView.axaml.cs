using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using BAP.Desktop.ViewModels;

namespace BAP.Desktop.Views;

public partial class TrackLayoutView : UserControl
{
    public TrackLayoutView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is TrackLayoutViewModel vm)
        {
            vm.Connections.CollectionChanged += OnConnectionsChanged;
            RedrawConnections(vm);
        }
    }

    private void OnConnectionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (DataContext is TrackLayoutViewModel vm)
            RedrawConnections(vm);
    }

    private void RedrawConnections(TrackLayoutViewModel vm)
    {
        var canvas = this.FindControl<Canvas>("ConnectionCanvas");
        if (canvas == null) return;

        canvas.Children.Clear();

        var mainBrush = new SolidColorBrush(Color.Parse("#334155"));
        var branchBrush = new SolidColorBrush(Color.Parse("#22D3EE"));

        foreach (var conn in vm.Connections)
        {
            var line = new Line
            {
                StartPoint = new Avalonia.Point(conn.X1, conn.Y1),
                EndPoint = new Avalonia.Point(conn.X2, conn.Y2),
                Stroke = conn.IsBranch ? branchBrush : mainBrush,
                StrokeThickness = conn.IsBranch ? 2 : 3,
                StrokeDashArray = conn.IsBranch ? [4, 3] : null,
                Opacity = 0.8
            };
            canvas.Children.Add(line);

            // Arrow head at the end
            DrawArrow(canvas, conn.X2, conn.Y2, conn.X1, conn.Y1,
                conn.IsBranch ? branchBrush : mainBrush);
        }
    }

    private static void DrawArrow(Canvas canvas, double tipX, double tipY,
        double fromX, double fromY, IBrush brush)
    {
        const double arrowSize = 8;
        double angle = Math.Atan2(tipY - fromY, tipX - fromX);

        var p1 = new Avalonia.Point(
            tipX - arrowSize * Math.Cos(angle - 0.4),
            tipY - arrowSize * Math.Sin(angle - 0.4));
        var p2 = new Avalonia.Point(
            tipX - arrowSize * Math.Cos(angle + 0.4),
            tipY - arrowSize * Math.Sin(angle + 0.4));

        var polygon = new Polygon
        {
            Points = [new Avalonia.Point(tipX, tipY), p1, p2],
            Fill = brush,
            Opacity = 0.8
        };
        canvas.Children.Add(polygon);
    }
}
