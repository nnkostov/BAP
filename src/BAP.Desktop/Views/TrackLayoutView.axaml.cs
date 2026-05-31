using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;
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

        // Draw dot grid background
        DrawDotGrid(canvas, 2000, 1200);

        // Draw connection paths
        var mainColor = Color.Parse("#334155");
        var mainGlow = Color.Parse("#1A5B8DEF");
        var branchColor = Color.Parse("#22D3EE");
        var branchGlow = Color.Parse("#2622D3EE");

        foreach (var conn in vm.Connections)
        {
            var color = conn.IsBranch ? branchColor : mainColor;
            var glowColor = conn.IsBranch ? branchGlow : mainGlow;

            // Glow layer (wider, semi-transparent)
            DrawBezierConnection(canvas, conn, new SolidColorBrush(glowColor),
                conn.IsBranch ? 8 : 10, null, 0.5);

            // Main line
            DrawBezierConnection(canvas, conn, new SolidColorBrush(color),
                conn.IsBranch ? 2.5 : 3.5,
                conn.IsBranch ? new AvaloniaList<double> { 6, 4 } : null, 0.9);

            // Arrow head
            DrawArrow(canvas, conn.X2, conn.Y2, conn.X1, conn.Y1,
                new SolidColorBrush(color), conn.IsBranch ? 7 : 9);
        }
    }

    private static void DrawDotGrid(Canvas canvas, double width, double height)
    {
        const double spacing = 24;
        var dotBrush = new SolidColorBrush(Color.Parse("#0D1A2B3D"));

        for (double x = 12; x < width; x += spacing)
        {
            for (double y = 12; y < height; y += spacing)
            {
                var dot = new Ellipse
                {
                    Width = 2,
                    Height = 2,
                    Fill = dotBrush
                };
                Canvas.SetLeft(dot, x);
                Canvas.SetTop(dot, y);
                canvas.Children.Add(dot);
            }
        }
    }

    private static void DrawBezierConnection(Canvas canvas, TrackConnectionViewModel conn,
        IBrush stroke, double thickness, AvaloniaList<double>? dashArray, double opacity)
    {
        double dx = conn.X2 - conn.X1;
        double dy = conn.Y2 - conn.Y1;
        double dist = Math.Sqrt(dx * dx + dy * dy);
        double curvature = Math.Min(dist * 0.4, 60);

        // Bezier control points
        double cx1 = conn.X1 + curvature;
        double cy1 = conn.Y1;
        double cx2 = conn.X2 - curvature;
        double cy2 = conn.Y2;

        // If going backwards (loop), use bigger arched control points
        if (dx < 0)
        {
            double archHeight = Math.Max(80, Math.Abs(dy) + 40);
            cx1 = conn.X1 + 60;
            cy1 = conn.Y1 + archHeight;
            cx2 = conn.X2 - 60;
            cy2 = conn.Y2 + archHeight;
        }

        var geometry = new PathGeometry();
        var figure = new PathFigure { StartPoint = new Point(conn.X1, conn.Y1), IsClosed = false };
        figure.Segments!.Add(new BezierSegment
        {
            Point1 = new Point(cx1, cy1),
            Point2 = new Point(cx2, cy2),
            Point3 = new Point(conn.X2, conn.Y2)
        });
        geometry.Figures!.Add(figure);

        var path = new Avalonia.Controls.Shapes.Path
        {
            Data = geometry,
            Stroke = stroke,
            StrokeThickness = thickness,
            StrokeDashArray = dashArray,
            StrokeLineCap = PenLineCap.Round,
            StrokeJoin = PenLineJoin.Round,
            Opacity = opacity
        };
        canvas.Children.Add(path);
    }

    private static void DrawArrow(Canvas canvas, double tipX, double tipY,
        double fromX, double fromY, IBrush brush, double size)
    {
        double angle = Math.Atan2(tipY - fromY, tipX - fromX);

        var p1 = new Point(
            tipX - size * Math.Cos(angle - 0.35),
            tipY - size * Math.Sin(angle - 0.35));
        var p2 = new Point(
            tipX - size * Math.Cos(angle + 0.35),
            tipY - size * Math.Sin(angle + 0.35));

        var polygon = new Polygon
        {
            Points = [new Point(tipX, tipY), p1, p2],
            Fill = brush,
            Opacity = 0.9
        };
        canvas.Children.Add(polygon);
    }
}
