using System.Collections.ObjectModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace Shot.Extensions
{
    public static class GraphExtension
    {
        public static ObservableCollection<ISeries> CreateGraph(ObservableCollection<double> observableValues, ObservableCollection<double> negavtiveObservableValues)
        {
            return new ObservableCollection<ISeries>()
            {
                GetColumnSeries(observableValues),
                GetColumnSeries(negavtiveObservableValues)
            };
        }

        public static ColumnSeries<double> GetColumnSeries(ObservableCollection<double> values)
        {
            return new ColumnSeries<double>
            {
                Values = values,
                Fill = new SolidColorPaint
                {
                    Color = SKColors.DarkRed,
                    StrokeCap = SKStrokeCap.Round,
                    StrokeThickness = 6
                },
                Pivot = 0,
                Stroke = new SolidColorPaint
                {
                    Color = SKColors.DarkRed,
                    StrokeCap = SKStrokeCap.Round,
                    StrokeThickness = 6
                },
                IgnoresBarPosition = true,
                GroupPadding = 0,
                MaxBarWidth = 2
            };
        }
    }
}
