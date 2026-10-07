using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

// Lecture simple des gâchettes des manettes (écrans de début / de fin, menu).
// Dans l'éditeur sans casque : la barre espace fait office de gâchette (nouveau Input System : le projet n'utilise que lui).
public static class VRInput
{
    static readonly List<InputDevice> devices = new List<InputDevice>();

    public static bool AnyTrigger()
    {
        devices.Clear();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller, devices);
        foreach (var d in devices)
            if (d.TryGetFeatureValue(CommonUsages.triggerButton, out bool b) && b) return true;
        return KeyboardSpace();
    }

    /// <summary>Gâchette d'une main précise (XRNode.LeftHand / RightHand).</summary>
    public static bool Trigger(XRNode hand)
    {
        var d = InputDevices.GetDeviceAtXRNode(hand);
        return d.isValid && d.TryGetFeatureValue(CommonUsages.triggerButton, out bool b) && b;
    }

    public static void Haptic(XRNode hand, float amplitude = 0.25f, float duration = 0.03f)
    {
        var d = InputDevices.GetDeviceAtXRNode(hand);
        if (d.isValid) d.SendHapticImpulse(0, amplitude, duration);
    }

    public static bool KeyboardSpace()
    {
        var k = UnityEngine.InputSystem.Keyboard.current;
        return k != null && k.spaceKey.isPressed;
    }
}
