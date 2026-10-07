using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

// Lecture simple des gâchettes des manettes (écran de début / de fin). Barre espace = gâchette dans l'éditeur sans casque.
public static class VRInput
{
    static readonly List<InputDevice> devices = new List<InputDevice>();

    public static bool AnyTrigger()
    {
        devices.Clear();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller, devices);
        foreach (var d in devices)
            if (d.TryGetFeatureValue(CommonUsages.triggerButton, out bool b) && b) return true;
        return Input.GetKey(KeyCode.Space);
    }
}
