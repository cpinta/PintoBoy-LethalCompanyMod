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
        static GameObject ljObject;

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

                    Debug.Log("Trying to spawn PintoBoy test objs");
                    pintoObject = Object.Instantiate(Pinto_ModBase.itemPintoBoyPrefab.spawnPrefab);
                    pintoObject.GetComponent<NetworkObject>().Spawn(true);
                    pintoObject.transform.position = new Vector3(3.0792f, 0.2899f, -14.6918f);
                    Debug.Log("PintoBoy test obj 1 hopefully spawned");

                    ljObject = Object.Instantiate(Pinto_ModBase.itemLJCartridgePrefab.spawnPrefab);
                    ljObject.GetComponent<NetworkObject>().Spawn(true);
                    ljObject.transform.position = new Vector3(2.6302f, 0.2875f, -14.4498f);

                    Debug.Log("PintoBoy test objs hopefully spawned");
                }
            }
            catch
            {
                Pinto_ModBase.Instance.Value.logger.LogError("Failed to instantiate network prefab!");
            }
        }
    }
}