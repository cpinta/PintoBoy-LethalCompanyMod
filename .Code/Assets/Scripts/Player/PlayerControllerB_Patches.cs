using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace PintoMod.Assets.Scripts
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    public static class PlayerControllerB_Patches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControllerB), "Start")]
        static void Start(PlayerControllerB __instance)
        {
            //__instance.ItemSlots[0] = Pinto_ModBase.pintoGrab;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        static void Update(PlayerControllerB __instance)
        {
            //if (!__instance.IsOwner) return;
            //if (!__instance.playerActions.Movement.ActivateItem.WasPressedThisFrame()) return;
            //Pinto_ModBase.Instance.Value.logger.LogInfo("1. Interact Pressed");
            //if (__instance.ItemSlots[__instance.currentItemSlot].name != "Pinto") return;
            //Pinto_ModBase.Instance.Value.logger.LogInfo("2. Pinto in hand. Pressing jump");

            ////__instance.ItemSlots[__instance.currentItemSlot].gameObject.GetComponent<PintoBoy>().Jump();
            //Pinto_ModBase.Instance.Value.logger.LogInfo("3. Pinto in hand. Jump Pressed");
        }
    }
}