using System.Collections;
using System.Diagnostics;
using System.Reflection;
using LabFusion.Network;
using LabFusion.Player;
using MelonLoader;
using UnityEngine;

namespace MashGamemodeLibrary.Execution;

public static class Executor
{
    public delegate void Runnable();

    private static void Run(Runnable runnable)
    {
#if DEBUG
        // Make sure the debugger catches the error
        try
        {
            runnable();
        }
        catch (Exception e)
        {
            MelonLogger.Error($"[Mash Gamemode Library] An error occurred during execution: {e}");
        }
#else
        runnable();
#endif
    }

    public static void RunIfHost(Runnable runnable, string? error = null)
    {
        if (!NetworkInfo.IsHost)
        {
            if (error == null) return;

            MelonLogger.Error($"This can only be ran from a host: {error}");
            return;
        }

#if DEBUG
        StepInHost();
#endif

        Run(runnable);

#if DEBUG
        StepOutHost();
#endif
    }

    public static void RunIfRemote(Runnable runnable, string? error = null)
    {
        if (NetworkInfo.IsHost)
        {
            if (error == null) return;

            MelonLogger.Error($"This can only be ran from a remote user: {error}");
            return;
        }

#if DEBUG
        StepInHost();
#endif

        Run(runnable);

#if DEBUG
        StepOutHost();
#endif
    }

    public static void RunIfNotMe(PlayerID id, Runnable runnable)
    {
        if (id.IsMe)
            return;

        Run(runnable);
    }

    public static void RunIfMe(PlayerID id, Runnable runnable)
    {
        if (!id.IsMe)
            return;

        Run(runnable);
    }

    // Validation
    public static void EnsureHost()
    {
#if DEBUG
        if (IsHostContext) return;

        var trace = new StackTrace();
        MelonLogger.Warning("Possibly executing a host only method on the client!", trace);
#endif
    }

#if DEBUG
    private static int _hostDepth;
    public static bool IsHostContext => _hostDepth == 0;

    private static void StepInHost()
    {
        _hostDepth += 1;
    }

    private static void StepOutHost()
    {
        _hostDepth = Math.Max(0, _hostDepth - 1);
    }
#endif

    private static bool IsInContext(ExecutionContext executionContext)
    {
        return executionContext switch
        {
            ExecutionContext.Host => NetworkInfo.IsHost,
            ExecutionContext.Remote => !NetworkInfo.IsHost,
            _ => throw new ArgumentOutOfRangeException(nameof(executionContext), executionContext, null)
        };
    }

    private static bool MayExecute<T>() where T : Delegate
    {
        // Special attributes
        var runIfAttribute = typeof(T).GetCustomAttribute<RunIf>();
        if (runIfAttribute != null)
            return IsInContext(runIfAttribute.ExecutionContext);

        return true;
    }

    /// <summary>
    /// WARNING: Not checked for network attributes
    /// </summary>
    /// <param name="action"></param>
    public static void RunUnchecked(Action action)
    {
        try
        {
            action.Invoke();
        }
        catch (Exception exception)
        {
            MelonLogger.Error("Failed to execute executor call", exception);
        }
    }
    
    public static void RunChecked<T>(T action) where T : Delegate
    {
        if (!MayExecute<T>())
            return;

        RunUnchecked(() => action.DynamicInvoke());
    }

    public static void RunChecked<T>(T action, object argument1) where T : Delegate
    {
        if (!MayExecute<T>())
            return;

        RunUnchecked(() => action.DynamicInvoke(argument1));
    }
    
    private static IEnumerator DelayThenInvoke<T>(T action, float timeout) where T : Delegate
    {
        yield return new WaitForSeconds(timeout);

        try
        {
            action.DynamicInvoke();
        }
        catch (Exception exception)
        {
            MelonLogger.Error("Failed to execute delayed executor call", exception);
        }
    }
    
    public static void RunCheckedInFuture<T>(T action, TimeSpan timeout) where T : Delegate
    {
        if (!MayExecute<T>())
            return;

        MelonCoroutines.Start(DelayThenInvoke(action, timeout.Seconds));
    }
    
    // Event Invocation
    
    public static void Try<T>(this T value, Action<T> action)
    {
        try
        {
            action(value);
        }
        catch (Exception exception)
        {
            MelonLogger.Error($"Failed to execute {typeof(T).FullName}", exception);
        }
    }

    public static TReturn Try<TInstance, TReturn>(this TInstance value, Func<TInstance, TReturn> action, Func<TReturn> defaultValue)
    {
        try
        {
            return action(value);
        }
        catch (Exception exception)
        {
            MelonLogger.Error($"Failed to execute {typeof(TInstance).FullName}", exception);
        }

        return defaultValue();
    }
    
    public static TReturn Try<TInstance, TReturn>(this TInstance value, Func<TInstance, TReturn> action, TReturn defaultValue)
    {
        return Try(value, action, () => defaultValue);
    }
}