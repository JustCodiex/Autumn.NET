using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autumn.Test.TestHelpers;

public static class TimedAssert {

    public static void True(Func<bool> predicate, long withinMillis = 5000) { 
        DateTime start = DateTime.Now;
        while ((DateTime.Now - start).TotalMilliseconds < withinMillis) {
            if (predicate()) {
                return;
            }
            Thread.Sleep(100);
        }
        Assert.Fail($"Predicate failed after {(DateTime.Now - start).TotalSeconds}s");
    }

}
