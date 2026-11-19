using System.Reflection;
using Project.Core;
using UnityEngine;

internal static class LogicTickerTests
{
    private static void InvokePrivate(object target, string methodName)
    {
        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        method.Invoke(target, null);
    }

    public static void Update_AccumulatesAndFiresOnTick_AtFixedRate()
    {
        ResetSingleton();

        var go = new GameObject("Ticker");
        LogicTicker ticker = go.AddComponent<LogicTicker>();

        int tickCount = 0;
        ticker.OnTick += _ => tickCount++;

        Time.deltaTime = LogicTicker.FixedDeltaTime * 2.5f;
        InvokePrivate(ticker, "Update");

        TestRunner.AssertEqual(2, tickCount, "2.5 fixed-steps worth of deltaTime should fire exactly 2 ticks");
    }

    internal static void ResetSingleton()
    {
        typeof(LogicTicker).GetProperty("Instance").SetValue(null, null);
    }
}
