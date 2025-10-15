using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using LabFusion.Extensions;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Registry;

public interface IRegistry<TValue> where TValue : class
{
    public ulong GetID(MemberInfo type);
    public ulong GetID<T>() where T : TValue;
    public ulong GetID<T>(T instance) where T : TValue;

    public void Register<T>(ulong id, T value) where T : TValue, new();
    public void Register<T>() where T : TValue, new();
    public void RegisterAll<T>();

    public TValue? Get(ulong id);
    public bool TryGet(ulong id, [MaybeNullWhen(false)] out TValue entry);
    public TValue? Get<T>() where T : TValue;
    public bool TryGet<T>([MaybeNullWhen(false)] out TValue entry) where T : TValue;
    
    public bool Contains(ulong id);
}