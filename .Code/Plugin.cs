using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using System.Security.Permissions;
using GameNetcodeStuff;
using UnityEngine;
using Unity.Netcode;
using LethalLib;
using System.Reflection;
using LethalLib.Modules;
using Object = UnityEngine.Object;
using HarmonyLib.Tools;
using BepInEx.Configuration;
using PintoMod.Assets.Scripts;
using PintoMod.Assets.Scripts.LethalJumpany;
using PintoMod.Assets.Scripts.FacilityDash;
using UnityEngine.UIElements.Layout;
using UnityEngine.InputSystem;
using System.ComponentModel;

namespace PintoMod
{
    [BepInPlugin(MODGUID, MODNAME, MODVERSION)]
    public class Pinto_ModBase : BaseUnityPlugin
    {
        public const string MODGUID = "Pinta.PintoBoy";
        public const string MODNAME = "PintoBoy";
        public const string MODVERSION = "2.0.0";

        private readonly Harmony harmony = new Harmony(MODGUID);

        public ManualLogSource logger;

        public static ConfigEntry<float>
            config_PintoboyRarity;

        public static readonly Lazy<Pinto_ModBase> Instance = new Lazy<Pinto_ModBase>(() => new Pinto_ModBase());

        public static AssetBundle pintoBundle;

        public static Item itemPintoBoyPrefab;
        public static Material matOffScreen;
        public static Material matOnScreen;

        public static Item itemPintoBoyLJ;
        public static Item itemLJCartridgePrefab;
        public static LethalJumpany gameLethalJumpanyPrefab;
        public static GameObject ljSpiderPrefab;
        public static GameObject ljSlimePrefab;
        public static GameObject ljLootbugPrefab;

        public static Item itemPintoBoyFD;
        public static Item itemFDCartridgePrefab;
        public static FacilityDash gameFacilityDashPrefab;
        public static GameObject fdBrackenPrefab;
        public static GameObject fdBunkerSpiderPrefab;
        public static GameObject fdLootBugPrefab;
        public static GameObject fdNutcrackerPrefab;
        public static GameObject fdSnareFleaPrefab;
        public static GameObject fdThumperPrefab;

        public int currentId = 0;


        public static string basePath = "assets/pintoboy";
        public static string devicePath = $"{basePath}/device";
        public static string gamesPath = $"{basePath}/games";
        public static string ljBasePath = $"{gamesPath}/lethal jumpany";
        public static string ljAudioPath = $"{ljBasePath}/audio/";
        public static string ljSpritesPath = $"{ljBasePath}/2d";


        public static string fdBasePath = $"{gamesPath}/facility dash";
        public static string fdAudioPath = $"{fdBasePath}/audio/";
        public static string fdSpritesPath = $"{fdBasePath}/sprites/monsters";

        private void Awake()
        {
            logger = BepInEx.Logging.Logger.CreateLogSource(MyPluginInfo.PLUGIN_GUID);



            ConfigSetup();
            LoadBundle();
            SetVariables();

            harmony.PatchAll(typeof(Pinto_ModBase));

            // Unity Netcode Weaver
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }

            Debug.Log("Pintoboy initialized");
        }

        private void ConfigSetup()
        {
            config_PintoboyRarity = Config.Bind("Pintoboy Rarity", "Value", 25f, "How rare is the PintoBoy");
        }

        private void SetVariables()
        {
            //LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(ljSpiderPrefab);
            //LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(ljSlimePrefab);
            //LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(ljLootbugPrefab);
            //LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(screenPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(itemPintoBoyPrefab.spawnPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(itemPintoBoyLJ.spawnPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(itemPintoBoyFD.spawnPrefab);

            Items.RegisterScrap(itemPintoBoyLJ, (int)config_PintoboyRarity.Value, Levels.LevelTypes.All);
            Items.RegisterScrap(itemPintoBoyFD, (int)config_PintoboyRarity.Value, Levels.LevelTypes.All);
        }

        private void LoadBundle()
        {
            pintoBundle = AssetBundle.LoadFromMemory(Properties.Resources.pintobund);
            if (pintoBundle == null) throw new Exception("Failed to load Pinto Bundle!");


            // PintoBoy prefabs
            itemPintoBoyPrefab = pintoBundle.LoadAsset<Item>($"{basePath}/pintoboy.asset");
            if (itemPintoBoyPrefab == null) throw new Exception("Failed to load Pinto Item!");
            PintoBoy pintoBoy = itemPintoBoyPrefab.spawnPrefab.AddComponent<PintoBoy>();
            if (pintoBoy == null) throw new Exception("Failed to load Pinto Boy!");
            pintoBoy.itemProperties = itemPintoBoyPrefab;

            matOffScreen = pintoBundle.LoadAsset<Material>($"{devicePath}/off screen.mat");
            if (matOffScreen == null) throw new Exception("Failed to load off screen material!");

            matOnScreen = pintoBundle.LoadAsset<Material>($"Screen Mat.mat");
            if (matOnScreen == null) throw new Exception("Failed to load Screen Mat material!");

            // Enemies
            GameObject ljSpider = pintoBundle.LoadAsset<GameObject>($"{ljSpritesPath}/spider/spider.prefab");
            if (ljSpider == null) throw new Exception("Failed to load Spider Prefab Object!");
            ljSpider.AddComponent<LJEnemy>();
            ljSpiderPrefab = ljSpider;
            if (ljSpiderPrefab == null) throw new Exception("Failed to load Spider Prefab!");

            GameObject ljSlime = pintoBundle.LoadAsset<GameObject>($"{ljSpritesPath}/slime/slime.prefab");
            if (ljSlime == null) throw new Exception("Failed to load Slime Prefab Object!");
            ljSlime.AddComponent<LJEnemy>();
            ljSlimePrefab = ljSlime;
            if (ljSlimePrefab == null) throw new Exception("Failed to load Slime Prefab!");

            GameObject ljLootbug = pintoBundle.LoadAsset<GameObject>($"{ljSpritesPath}/loot bug/loot bug.prefab");
            if (ljLootbug == null) throw new Exception("Failed to load Lootbug Prefab Object!");
            ljLootbug.AddComponent<LJEnemy>();
            ljLootbugPrefab = ljLootbug;
            if (ljLootbugPrefab == null) throw new Exception("Failed to load Lootbug Prefab!");

            // Enemies
            string prefabName = "";
            AudioClip acDefaultAttack = Pinto_ModBase.GetAudioClip(Pinto_ModBase.fdAudioPath + "67_knock (Player hit)");

            // Bracken
            prefabName = "Bracken";
            fdBrackenPrefab = pintoBundle.LoadAsset<GameObject>($"{fdSpritesPath}/{prefabName}.prefab");
            if (fdBrackenPrefab == null) throw new Exception("PintoBoy FD: Failed to load " + prefabName + " Prefab Object!");
            Debug.Log($"PintoBoy FD: Loading {prefabName} Prefab");
            fdBrackenPrefab.AddComponent<FD_Bracken>();

            // Bunker Spider
            prefabName = "Bunker Spider";
            fdBunkerSpiderPrefab = pintoBundle.LoadAsset<GameObject>($"{fdSpritesPath}/{prefabName}.prefab");
            if (fdBunkerSpiderPrefab == null) throw new Exception("PintoBoy FD: Failed to load " + prefabName + " Prefab Object!");
            Debug.Log($"PintoBoy FD: Loading {prefabName} Prefab");
            fdBunkerSpiderPrefab.AddComponent<FD_BunkerSpider>();

            // Loot Bug
            prefabName = "Loot Bug";
            fdLootBugPrefab = pintoBundle.LoadAsset<GameObject>($"{fdSpritesPath}/{prefabName}.prefab");
            if (fdLootBugPrefab == null) throw new Exception("PintoBoy FD: Failed to load " + prefabName + " Prefab Object!");
            Debug.Log($"PintoBoy FD: Loading {prefabName} Prefab");
            fdLootBugPrefab.AddComponent<FD_LootBug>();

            // Snare Flea
            prefabName = "Snare Flea";
            fdSnareFleaPrefab = pintoBundle.LoadAsset<GameObject>($"{fdSpritesPath}/{prefabName}.prefab");
            if (fdSnareFleaPrefab == null) throw new Exception("PintoBoy FD: Failed to load " + prefabName + " Prefab Object!");
            Debug.Log($"PintoBoy FD: Loading {prefabName} Prefab");
            fdSnareFleaPrefab.AddComponent<FD_SnareFlea>();

            // Thumper
            prefabName = "Thumper";
            fdThumperPrefab = pintoBundle.LoadAsset<GameObject>($"{fdSpritesPath}/{prefabName}.prefab");
            if (fdThumperPrefab == null) throw new Exception("PintoBoy FD: Failed to load " + prefabName + " Prefab Object!");
            Debug.Log($"PintoBoy FD: Loading {prefabName} Prefab");
            FD_Thumper thumper = fdThumperPrefab.AddComponent<FD_Thumper>();
            thumper.acEntrance = Pinto_ModBase.GetAudioClip(Pinto_ModBase.fdAudioPath + "monster sounds/thumper yell");
            thumper.acDefaultAttack = acDefaultAttack;

            // Nutcracker
            prefabName = "Nutcracker";
            fdNutcrackerPrefab = pintoBundle.LoadAsset<GameObject>($"{fdSpritesPath}/{prefabName}.prefab");
            if (fdNutcrackerPrefab == null) throw new Exception("PintoBoy FD: Failed to load " + prefabName + " Prefab Object!");
            Debug.Log($"PintoBoy FD: Loading {prefabName} Prefab");
            fdNutcrackerPrefab.AddComponent<FD_Nutcracker>();

            itemPintoBoyLJ = pintoBundle.LoadAsset<Item>($"{basePath}/pintoboy lj.asset");
            if (itemPintoBoyLJ == null) throw new Exception("Failed to load Pinto LJ Item!");
            PintoBoy pintoBoyLJ = itemPintoBoyLJ.spawnPrefab.AddComponent<LethalJumpany>();
            if (pintoBoyLJ == null) throw new Exception("Failed to load Pinto Boy!");
            pintoBoyLJ.itemProperties = itemPintoBoyLJ;

            itemPintoBoyFD = pintoBundle.LoadAsset<Item>($"{basePath}/pintoboy fd.asset");
            if (itemPintoBoyFD == null) throw new Exception("Failed to load Pinto FD Item!");
            FacilityDash pintoBoyFD = itemPintoBoyFD.spawnPrefab.AddComponent<FacilityDash>();
            if (pintoBoyFD == null) throw new Exception("Failed to load Pinto Boy!");
            pintoBoyFD.itemProperties = itemPintoBoyFD;
            pintoBoyFD.prefabBracken = fdBrackenPrefab;
            pintoBoyFD.prefabBunkerSpider = fdBunkerSpiderPrefab;
            pintoBoyFD.prefabLootBug = fdLootBugPrefab;
            pintoBoyFD.prefabNutcracker = fdNutcrackerPrefab;
            pintoBoyFD.prefabSnareFlea = fdSnareFleaPrefab;
            pintoBoyFD.prefabThumper = fdThumperPrefab;
        }

        GameObject LoadFDPrefab(MonoBehaviour component, string prefabName)
        {
            GameObject gameObject = pintoBundle.LoadAsset<GameObject>($"{fdSpritesPath}/{prefabName}.prefab");
            if (gameObject == null) throw new Exception("Failed to load " + prefabName + " Prefab Object!");
            Debug.Log($"PintoBoy FD: Loading {prefabName} Prefab. Component type is:" + component.GetType());
            gameObject.AddComponent(component.GetType());
            return gameObject;
        }

        void LoadPrefabObject(GameObject prefab, MonoBehaviour component, string prefabName)
        {
            prefab = pintoBundle.LoadAsset<GameObject>($"{fdSpritesPath}/{prefabName}.prefab");
            if (prefab == null) throw new Exception("Failed to load " + prefabName + " Prefab Object!");
            prefab.AddComponent(component.GetType());
            if (prefab == null) throw new Exception("Failed to load " + prefabName + " Prefab!");
        }

        public static AudioClip GetAudioClip(string path)
        {
            AudioClip clip = pintoBundle.LoadAsset<AudioClip>(path + ".wav");
            if (clip == null)
            {
                clip = pintoBundle.LoadAsset<AudioClip>(path + ".mp3");
            }
            if (clip == null)
            {
                throw new Exception($"Failed to load Audio Clip {path}");
            }

            return clip;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.CollectNewScrapForThisRound))]
        public static void PintoBoyAddedToShip(GrabbableObject scrapObject)
        {
            if (scrapObject is PintoBoy)
            {
                PintoBoy pinto = (PintoBoy)scrapObject;
                pinto.MakeScreenNOTSpawnable();
                Debug.Log("PintoBoy added to ship and screen not spawned");
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.DespawnPropsAtEndOfRound))]
        public static void DespawnPintoBoyScreens(RoundManager __instance, ref bool despawnAllItems)
        {
            if (!__instance.IsServer)
            {
                return;
            }
            PintoBoy[] pintoBoyArray = Object.FindObjectsOfType<PintoBoy>();
            for(int i = 0; i < pintoBoyArray.Length; i++)
            {
                if (despawnAllItems || (!pintoBoyArray[i].isHeld && !pintoBoyArray[i].isInShipRoom))
                {
                    if (pintoBoyArray[i].isHeld && pintoBoyArray[i].playerHeldBy != null)
                    {
                        pintoBoyArray[i].playerHeldBy.DropAllHeldItemsAndSync();
                    }
                    NetworkObject component = pintoBoyArray[i].gameObject.GetComponent<NetworkObject>();
                    if (component != null && component.IsSpawned)
                    {
                        Object.Destroy(pintoBoyArray[i].cam.gameObject);
                        pintoBoyArray[i].gameObject.GetComponent<NetworkObject>().Despawn(true);
                    }
                    else
                    {
                        Debug.Log("Error/warning: prop '" + pintoBoyArray[i].gameObject.name + "' was not spawned or did not have a NetworkObject component! Skipped despawning and destroyed it instead.");
                        Object.Destroy(pintoBoyArray[i].cam.gameObject);
                        Object.Destroy(pintoBoyArray[i].gameObject);
                    }
                }
                else
                {
                    pintoBoyArray[i].scrapPersistedThroughRounds = true;
                }
                if (__instance.spawnedSyncedObjects.Contains(pintoBoyArray[i].gameObject))
                {
                    __instance.spawnedSyncedObjects.Remove(pintoBoyArray[i].gameObject);
                }
            }
        }
    }
}
