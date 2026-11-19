using System;
using System.Collections.Generic;

internal static class TestRunner
{
    private static int _pass;
    private static int _fail;

    public static void Assert(bool condition, string message)
    {
        if (condition) _pass++;
        else { _fail++; Console.WriteLine($"FAIL: {message}"); }
    }

    public static void AssertEqual<T>(T expected, T actual, string message)
    {
        if (Equals(expected, actual)) _pass++;
        else { _fail++; Console.WriteLine($"FAIL: {message} (expected {expected}, got {actual})"); }
    }

    public static int Run(IEnumerable<(string Name, Action Test)> tests)
    {
        foreach ((string name, Action test) in tests)
        {
            try
            {
                test();
            }
            catch (Exception e)
            {
                _fail++;
                Console.WriteLine($"EXCEPTION in {name}: {e}");
            }
        }

        Console.WriteLine($"\n{_pass} passed, {_fail} failed.");
        return _fail == 0 ? 0 : 1;
    }
}
