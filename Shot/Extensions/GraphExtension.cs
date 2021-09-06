using System.Collections.Generic;
using System.Collections.ObjectModel;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace Shot.Extensions
{
    public static class GraphExtension
    {
        public static ObservableCollection<ISeries> CreateGraph(ObservableCollection<double> observableValues)
        {
            return new ObservableCollection<ISeries>()
            {
                new LineSeries<double>
                {
                    Values = observableValues,
                    Fill = null,
                    Pivot = 0,
                    LineSmoothness = 0.3,
                    GeometrySize = 0,
                    Stroke = new SolidColorPaint
                    {
                        Color = SKColors.DarkRed,
                        StrokeCap = SKStrokeCap.Round,
                        StrokeThickness = 6
                    },

                }
            };
        }
    }
}
