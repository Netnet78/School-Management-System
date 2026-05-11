using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System.Collections.ObjectModel;

namespace SchoolManagement.Presentation.Shared.Observables
{
    public class ChartObservableModel : ObservableObject
    {
        public ChartObservableModel()
        {
            Series = new ObservableCollection<ISeries>();
            Series.CollectionChanged += (s, e) => OnPropertyChanged(nameof(IsEmpty));
        }

        public ObservableCollection<ISeries> Series { get; }

        // True when there's no non-zero data in any series
        public bool IsEmpty => !Series.Any(series =>
            series.Values != null && series.Values.Cast<object?>().Any(v => v != null && !v.Equals(0)));

        private Axis[] _xAxes = Array.Empty<Axis>();
        public Axis[] XAxes
        {
            get => _xAxes;
            set
            {
                if (_xAxes != value)
                {
                    _xAxes = value;
                    OnPropertyChanged(nameof(XAxes));
                }
            }
        }

        private Axis[] _yAxes = Array.Empty<Axis>();
        public Axis[] YAxes
        {
            get => _yAxes;
            set
            {
                if (_yAxes != value)
                {
                    _yAxes = value;
                    OnPropertyChanged(nameof(YAxes));
                }
            }
        }
    }

}
