using MelonLoader;

namespace MashGamemodeLibrary.Util;

public static class SafeEvents
{
    public static void InvokeSafely<T>(this T value, Action<T> action)
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
    
    public static TReturn InvokeSafely<TInstance, TReturn>(this TInstance value, TReturn defaultValue, Func<TInstance, TReturn> action)
    {
        try
        {
            return action(value);
        }
        catch (Exception exception)
        {
            MelonLogger.Error($"Failed to execute {typeof(TInstance).FullName}", exception);
        }

        return defaultValue;
    }
}