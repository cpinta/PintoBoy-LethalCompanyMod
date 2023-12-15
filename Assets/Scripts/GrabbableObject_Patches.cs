using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace PintoMod.Assets.Scripts
{
    [HarmonyPatch(typeof(GrabbableObject))]
    public static class GrabbableObject_Patches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GrabbableObject), "Start")]
        static void Start(GrabbableObject __instance)
        {
            if (__instance.name == "Pinto")
            {
                //
                Pinto_ModBase.Instance.Value.logger.LogInfo("Instance is Pinto");
                Debug.Log("Instance is Pinto");
                PintoBoy pinto = (PintoBoy)__instance;
                Pinto_ModBase.Instance.Value.logger.LogInfo("Is something null? "+ pinto.IsAnythingNull());
                Debug.Log("Is something null? " + pinto.IsAnythingNull());
            }
        }
    }
}