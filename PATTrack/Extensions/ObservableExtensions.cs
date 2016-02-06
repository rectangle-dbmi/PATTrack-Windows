using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PATTrack.Extensions
{
    public static class ObservableExtensions
    {
        internal static IObservable<T> ResetTimer<T>(this IObservable<T> input, TimeSpan dueTime, TimeSpan period)
        {
            var timers = from i in input
                         select from x in Observable.Timer(dueTime, period)
                                select i;
            return timers.Switch();
        }
    }
}
