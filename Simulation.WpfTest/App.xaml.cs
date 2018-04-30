using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Simulation.WpfTest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IObservable<Unit> Frame { get; } = Observable.FromEventPattern(h => CompositionTarget.Rendering += h, h => CompositionTarget.Rendering -= h).Select(_ => Unit.Default);
    }
}
