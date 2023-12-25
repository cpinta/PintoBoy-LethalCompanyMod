using HarmonyLib;
using PintoMod;
using Unity.Netcode;
using UnityEngine;

namespace PintoMod.Assets.Scripts
{
    [HarmonyPatch]
    public class NetworkHandler
    {
        private static GameObject pintoObject;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameNetworkManager), "Start")]
        static void Init()
        {
            //Pinto_ModBase.Instance.Value.logger.LogError("Pintoboy NetworkHandler Init starting");
            Debug.Log("Pintoboy NetworkHandler Init starting");
            //NetworkManager.Singleton.AddNetworkPrefab(Pinto_ModBase.pintoPrefab);
            Debug.Log("Pintoboy NetworkHandler Init ending");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(StartOfRound), "Awake")]
        static void SpawnNetworkPrefab()
        {
            try
            {
                if (NetworkManager.Singleton.IsServer)
                {
                    //pintoObject = Object.Instantiate(Pinto_ModBase.pintoPrefab);
                    //pintoObject.GetComponent<NetworkObject>().Spawn(true);
                    //pintoObject.transform.position = new Vector3(0, 1.7927f, -13.9836f);
                }
            }
            catch
            {
                Pinto_ModBase.Instance.Value.logger.LogError("Failed to instantiate network prefab!");
            }
        }
    }
}