using LabFusion.Marrow.Proxies;
#if MELONLOADER
using MelonLoader;

using Il2CppInterop.Runtime.Attributes;
using UnityEngine;
#endif
namespace MashGamemodeLibrary.Config.Menu.Element;

#if MELONLOADER
[RegisterTypeInIl2Cpp]
#endif
public class ExtendedFloatElement : ValueElement
{
#if MELONLOADER
    public ExtendedFloatElement(IntPtr intPtr) : base(intPtr) { }

    private float _value = 0f;
    public float Value
    {
        get
        {
            return _value;
        }
        set
        {
            _value = value;

            Draw();

            OnValueChanged?.Invoke(value);
        }
    }

    public Func<float, bool> Validator { get; set; } = _ => true;
    public float Increment { get; set; } = 0.01f;

    public Action<float> OnValueChanged;

    public void NextValue() 
    {
        var newValue = Value + Increment;

        if (!Validator.Invoke(newValue))
            return;

        Value = newValue;
    }

    public void PreviousValue()
    {
        var newValue = Value - Increment;

        if (!Validator.Invoke(newValue))
            return;

        Value = newValue;
    }

    [HideFromIl2Cpp]
    public override object GetValue()
    {
        return Mathf.Round(Value * 1000f) / 1000f;
    }

    protected override void OnClearValues()
    {
        _value = 0f;

        Validator = _ => true;
        Increment = 0.01f;

        OnValueChanged = null;

        base.OnClearValues();
    }
#else
    public void NextValue()
    {

    }

    public void PreviousValue()
    {

    }
#endif
}