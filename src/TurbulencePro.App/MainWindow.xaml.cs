using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using TurbulencePro.Core.Turbulence;

namespace TurbulencePro.App;

public partial class MainWindow : Window
{
    private readonly MainViewModel _model = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _model;
        CompositionTarget.Rendering += (_, _) => DrawGraph();
    }

    private void DrawGraph()
    {
        Graph.Children.Clear();
        var w = Graph.ActualWidth;
        var h = Graph.ActualHeight;
        if (w < 10 || h < 10) return;
        Draw(_model.MeasuredHistory, w, h, Color.FromRgb(80, 170, 255));
        Draw(_model.SimulatedHistory, w, h, Color.FromRgb(255, 180, 60));
        Draw(_model.FinalHistory, w, h, Color.FromRgb(120, 220, 140));
    }

    private void Draw(System.Collections.ObjectModel.ObservableCollection<double> series, double w, double h, Color color)
    {
        if (series.Count < 2) return;
        var figure = new Polyline { Stroke = new SolidColorBrush(color), StrokeThickness = 2 };
        for (var i = 0; i < series.Count; i++)
        {
            var x = i / (double)Math.Max(series.Count - 1, 1) * w;
            var y = h - series[i] * (h - 8) - 4;
            figure.Points.Add(new Point(x, y));
        }
        Graph.Children.Add(figure);
    }

    private void ForceLight(object sender, RoutedEventArgs e) => _model.Force(TurbulenceClass.Light);
    private void ForceModerate(object sender, RoutedEventArgs e) => _model.Force(TurbulenceClass.Moderate);
    private void ForceSevere(object sender, RoutedEventArgs e) => _model.Force(TurbulenceClass.Severe);
    private void ForceCat(object sender, RoutedEventArgs e) => _model.Force(TurbulenceClass.ClearAirStatistical);
    private void ForceMechanical(object sender, RoutedEventArgs e) => _model.Force(TurbulenceClass.Mechanical);
    private void ForceWave(object sender, RoutedEventArgs e) => _model.Force(TurbulenceClass.MountainWave);
    private void ForceConvective(object sender, RoutedEventArgs e) => _model.Force(TurbulenceClass.Convective);
    private void ForceThermal(object sender, RoutedEventArgs e) => _model.Force(TurbulenceClass.Thermal);
    private void ForceGround(object sender, RoutedEventArgs e) => _model.Force(TurbulenceClass.Ground);
    private void ClearForce(object sender, RoutedEventArgs e) => _model.ClearForce();
}
