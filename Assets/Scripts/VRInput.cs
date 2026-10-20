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

    /// <summary>Bouton menu (☰) de la manette gauche ; dans l'éditeur sans casque : touche Échap.</summary>
    public static bool LeftMenu()
    {
        var d = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        if (d.isValid && d.TryGetFeatureValue(CommonUsages.menuButton, out bool b) && b) return true;
        var k = UnityEngine.InputSystem.Keyboard.current;
        return k != null && k.escapeKey.isPressed;
    }

    public static void Haptic(XRNode hand, float amplitude = 0.25f, float duration = 0.03f)
    {
        var d = InputDevices.GetDeviceAtXRNode(hand);
        if (d.isValid) d.SendHapticImpulse(0, amplitude, duration);
    }

    /// <summary>Vibration des deux manettes (coup reçu).</summary>
    public static void HapticBoth(float amplitude, float duration)
    {
        Haptic(XRNode.LeftHand, amplitude, duration);
        Haptic(XRNode.RightHand, amplitude, duration);
    }

    /// <summary>Main d'un interacteur XRI (son nom ou celui d'un parent contient « left » ou « right »).</summary>
    public static XRNode HandOf(Component interactor)
    {
        for (var c = interactor ? interactor.transform : null; c != null; c = c.parent)
            if (c.name.IndexOf("left", System.StringComparison.OrdinalIgnoreCase) >= 0) return XRNode.LeftHand;
            else if (c.name.IndexOf("right", System.StringComparison.OrdinalIgnoreCase) >= 0) return XRNode.RightHand;
        return XRNode.RightHand;
    }

    /// <summary>Bouton B de la manette droite ; dans l'éditeur sans casque : touche Entrée (et B en version PC).</summary>
    public static bool RightB()
    {
        var d = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (d.isValid && d.TryGetFeatureValue(CommonUsages.secondaryButton, out bool b) && b) return true;
        var k = UnityEngine.InputSystem.Keyboard.current;
        return k != null && (k.enterKey.isPressed || DesktopMode.On && k.bKey.isPressed);
    }

    /// <summary>Joystick gauche enfoncé (clic).</summary>
    public static bool LeftStickClick()
    {
        var d = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        return d.isValid && d.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool b) && b;
    }

    /// <summary>Touche du clavier (éditeur sans casque).</summary>
    public static bool KeyboardKey(UnityEngine.InputSystem.Key key)
    {
        var k = UnityEngine.InputSystem.Keyboard.current;
        return k != null && k[key].isPressed;
    }

    public static bool KeyboardSpace()
    {
        var k = UnityEngine.InputSystem.Keyboard.current;
        return k != null && k.spaceKey.isPressed;
    }
}
