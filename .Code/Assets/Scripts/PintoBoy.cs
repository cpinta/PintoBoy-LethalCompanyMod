//using LC_API.GameInterfaceAPI;
using LethalLib.Modules;
using PintoMod;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine.UIElements.UIR;
using System;
using Random = UnityEngine.Random;
using PintoMod.Assets.Scripts;
using static UnityEngine.UIElements.StylePropertyAnimationSystem;
//using System.Numerics;



public enum FadeState
{
    FadeOff = 0,
    FadeOn = 1,
    FadeIn = 2,
    FadeOut = 3
}

public class PintoBoy : GrabbableObject
{
    AudioSource audioSource;

    PintoBoyGame currentGame;

    Animator fadeAnim;

    string FadeString = "Fade";
    string DoAnimString = "DoAnim";

    GameObject cam;
    Transform trCam2DScene;
    GameObject modelScreen;
    Transform cartridgeLocation;
    Animator buttonAnim;

    ScanNodeProperties scanNodeProperties;
    public float jumpHeight = 9.75f;
    public float fastFallSpeed = 15f;
    public float rayCastDistance = 0.5f;
    public float rayCastOffset = -0.05f;

    public bool pressButton = false;

    public NetworkVariable<int> screenId = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    bool spawnScreen = true;

    Material matRenderTex = null;
    RenderTexture texRenderTex = null;

    public bool isTurnedOff = true;
    public bool isPaused = false;

    Renderer rendModelScreen;

    float batteryDischargeRate = 0.002f;

    // Start is called before the first frame update
    void Awake()
    {
        audioSource = this.GetComponent<AudioSource>();

        mainObjectRenderer = transform.Find("Model").GetComponent<MeshRenderer>();

        scanNodeProperties = this.GetComponentInChildren<ScanNodeProperties>();
        if(scanNodeProperties == null)
        {
            Debug.Log("scanNodePoperties null af tbh");
        }

        useCooldown = 0.1f;

        grabbable = true;
        parentObject = this.transform;

        Debug.Log("scannode start");
        grabbableToEnemies = true;
        scanNodeProperties.maxRange = 100;
        scanNodeProperties.minRange = 1;
        scanNodeProperties.requiresLineOfSight = true;
        scanNodeProperties.headerText = "PintoBoy";
        scanNodeProperties.subText = "PintoBoy Subtext";
        scanNodeProperties.creatureScanID = -1;
        scanNodeProperties.nodeType = 2;


        Debug.Log("scannode done");
        modelScreen = transform.Find("Model/Screen").gameObject;
        buttonAnim = transform.Find("Model/Button").GetComponent<Animator>();

        startFallingPosition = new Vector3(0, 0, 0);
        targetFloorPosition = new Vector3(0, 0, 0);

        insertedBattery = new Battery(false, 100);
        EnableItemMeshes(true);

        cartridgeLocation = mainObjectRenderer.transform.Find("Cartridge");

        Debug.Log("cartLoc.childcount:" + cartridgeLocation.childCount);
        if(cartridgeLocation.childCount == 0)
        {
            currentGame = null;
        }
    }

    void LateUpdate()
    {
        try
        {
            base.LateUpdate();
        }
        catch (System.Exception ex)
        {
            Debug.Log("Error in PintoBoy LateUpdate: "+ex);
        }
    }



    // Update is called once per frame
    void Update()
    {
        base.Update();

        if (spawnScreen)
        {
            SpawnScreen();
            spawnScreen = false;

            cam.transform.parent = null;
            cam.GetComponent<Camera>().orthographicSize = 2;


            cam.transform.position = transform.position + (Vector3.down * 400);
            cam.transform.rotation = Quaternion.identity;
            cam.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

            if(cam.transform.position.x == 0 || cam.transform.position.z == 0)
            {
                cam.transform.position = new Vector3(Random.Range(-500f, 500f), cam.transform.position.y, Random.Range(-500f, 500f));
            }


        }



        if (isTurnedOff)
        {
            return;
        }
        else
        {
            insertedBattery.charge -= batteryDischargeRate * Time.deltaTime;
        }

        //if(fadeAnim == null)
        //{
        //    Debug.Log("FadeAnim is null");
        //}
        //else
        //{
        //    Debug.Log("FadeAnim isnt null");
        //}

        if (fadeAnim.GetBool(DoAnimString))
        {
            fadeAnim.SetBool(DoAnimString, false);
        }

        if(currentGame != null && currentGame.cartridge != null)
        {
            InGameUpdate();
        }


        //for testing within the Unity Editor
        if (pressButton)
        {
            ButtonPress();
            pressButton = false;
        }
    }

    void InGameUpdate()
    {
        currentGame.GameUpdate();
    }

    public override void ItemInteractLeftRight(bool right)
    {
        base.ItemInteractLeftRight(right);

        if (!right)
        {
            ToggleOnOff();
        }
        else
        {
            int firstSlotWGame = FirstItemSlotWithGame();
            if (firstSlotWGame > -1 && firstSlotWGame < playerHeldBy.ItemSlots.Length)
            {
                InsertGame((PintoBoyCartridge)playerHeldBy.ItemSlots[firstSlotWGame], firstSlotWGame);
            }
        }
    }

    public override void DiscardItem()
    {
        if (playerHeldBy != null)
        {
            playerHeldBy.equippedUsableItemQE = false;
        }
        isBeingUsed = false;
        base.DiscardItem();
    }


    public override void EquipItem()
    {
        base.EquipItem();
        playerHeldBy.equippedUsableItemQE = true;
        ChangeOwnershipOfProp(playerHeldBy.playerClientId);
    }

    public void SetFade(FadeState state)
    {
        switch (state)
        {
            case FadeState.FadeOff:
                fadeAnim.SetInteger(FadeString, (int)FadeState.FadeOff);
                fadeAnim.SetBool(DoAnimString, true);
                break;
            case FadeState.FadeOn:
                fadeAnim.SetInteger(FadeString, (int)FadeState.FadeOn);
                fadeAnim.SetBool(DoAnimString, true);
                break;
            case FadeState.FadeIn:
                fadeAnim.SetInteger(FadeString, (int)FadeState.FadeIn);
                fadeAnim.SetBool(DoAnimString, true);
                break;
            case FadeState.FadeOut:
                fadeAnim.SetInteger(FadeString, (int)FadeState.FadeOut);
                fadeAnim.SetBool(DoAnimString, true);
                break;
        }
    }

    void ButtonPress()
    {

        Debug.Log("Button Press PintoBoy. isTurnedOff = " + isTurnedOff);
        buttonAnim.SetTrigger("Press");
        if (isTurnedOff)
        {
            return;
        }

        Debug.Log("Button Press PintoBoy. Currentgame = "+currentGame);

        if(currentGame != null)
        {
            Debug.Log("Button Press PintoBoy. Currentgame = " + currentGame);
            currentGame.ButtonPress();
        }

    }

    void TogglePause()
    {
        if (isPaused)
        {
            UnPause();
        }
        else
        {
            Pause();
        }
    }

    void Pause()
    {
        if (isTurnedOff)
        {
            return;
        }

        isPaused = true;
        SetScreenToOffTexture();

        if(currentGame != null)
        {
            currentGame.Pause();
        }

        audioSource.Stop();
    }

    void UnPause()
    {
        if(!isTurnedOff)
        {
            return;
        }

        isPaused = false;

        if (currentGame != null)
        {
            currentGame.UnPause();
        }
    }

    void ToggleOnOff()
    {
        if (isTurnedOff)
        {
            TurnOn();
        }
        else
        {
            TurnOff();
        }
    }

    void TurnOff()
    {
        if (isTurnedOff)
        {
            return;
        }

        isTurnedOff = true;
        SetScreenToOffTexture();

        if(currentGame != null)
        {
            currentGame.TurnedOn();
        }

        audioSource.Stop();
    }

    void TurnOn()
    {
        if (!isTurnedOff)
        {
            return;
        }

        isTurnedOff = false;
        SetScreenToRenderTexture();

        if(currentGame != null)
        {
            currentGame.TurnedOff();
        }
    }

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);

        ButtonPress();

        Debug.Log("Pinto Button pressed");
    }

    public override void UseUpBatteries()
    {
        base.UseUpBatteries();
        TurnOff();
    }

    public float Remap(float from, float fromMin, float fromMax, float toMin, float toMax)
    {
        return 0;
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    public void StopSounds()
    {
        audioSource.Stop();
    }

    public void EnableFade(bool enabled)
    {
        fadeAnim.enabled = enabled;
    }

    public void MakeScreenNOTSpawnable()
    {
        spawnScreen = false;
    }

    [ServerRpc]
    void SpawnScreenServerRpc()
    {
        Debug.Log("SpawnScreenServerRpc Called");
        int currentId = Pinto_ModBase.Instance.Value.currentId;
        PintoBoy[] boys = (PintoBoy[])FindObjectsOfType(typeof(PintoBoy));
        for(int i = currentId; i < boys.Length + currentId; i++)
        {
            Debug.Log("SpawnScreenServerRpc Called");
            boys[i].screenId.Value = i;
        }//
        Pinto_ModBase.Instance.Value.currentId += boys.Length;
        SpawnScreenClientRpc();
    }

    [ClientRpc]
    void SpawnScreenClientRpc()
    {
        Debug.Log("SpawnScreenClientRpc Called");
        SpawnScreen();
    }


    void SpawnScreen()
    {
        if(cam != null)
        {
            return;
        }

        Debug.Log("Spawning Screen");
        cam = transform.Find("2D Cam").gameObject;
        if(cam == null)
        {
            Debug.Log("Screen in Pintoboy is null even after instantiate");
        }

        SetScreenToRenderTexture();

        trCam2DScene = cam.transform.Find("2D Scene");

        fadeAnim = trCam2DScene.transform.Find("Fade").GetComponent<Animator>();
        fadeAnim.gameObject.SetActive(true);


        if (currentGame != null)
        {
            currentGame.InitializeObjects(cam.transform.Find("2D Scene/Game"));
        }

        spawnScreen = false;
    }

    void SetScreenToRenderTexture()
    {
        Debug.Log("SetScreenRendTex: checking if texRenderTex null");
        if (texRenderTex == null)
        {

            Debug.Log("SetScreenRendTex: texRenderTex is null. Setting");
            texRenderTex = new RenderTexture(160, 160, 16);

            // Step 1: Create a Render Texture
            texRenderTex.name = "PintoBoyScreen";
            texRenderTex.filterMode = FilterMode.Point;

            cam.GetComponent<Camera>().targetTexture = texRenderTex;
        }
        Debug.Log("SetScreenRendTex: checking if matRenderTex null");
        if (matRenderTex == null)
        {
            Debug.Log("SetScreenRendTex: matRenderTex is null. Setting");
            matRenderTex = Instantiate(Pinto_ModBase.matOnScreen);
            // Step 3: Assign the Unique Render Texture to the Prefab Instance
            matRenderTex.SetTexture("_MainTex", texRenderTex);
            matRenderTex.mainTexture = texRenderTex;
            // Step 4: Use Render Texture in Shader or Camera

        }

        Debug.Log("SetScreenRendTex: checking if rendModelScreen is null");
        if (rendModelScreen == null)
        {
            Debug.Log("SetScreenRendTex: rendModelScreen is null");
            rendModelScreen = modelScreen.GetComponent<Renderer>();
        }


        Debug.Log("SetScreenRendTex: setting rendModelScreen to matRenderTex");
        rendModelScreen.material = matRenderTex;
    }

    void SetScreenToOffTexture()
    {
        if (rendModelScreen == null)
        {
            rendModelScreen = modelScreen.GetComponent<Renderer>();
        }

        rendModelScreen.material = Pinto_ModBase.matOffScreen;
    }

    public void InsertGame(PintoBoyCartridge cart, int slotIndex)
    {
        try
        {

            Debug.Log("Inserting game: " + cart);


            Debug.Log("currentGame = " + currentGame);
            PintoBoyCartridge newCart = PintoBoyCartridge.Instantiate(cart, cart.transform.position, cart.transform.rotation, cart.transform.parent);

            Debug.Log($"scrapvalues: old:{cart.scrapValue}, new:{newCart.scrapValue}");
            Debug.Log($"playerheldby: old:{cart.playerHeldBy.name}, new:{newCart.playerHeldBy.name}");

            newCart.game = cart.game;

            newCart.scanNodeProperties = cart.scanNodeProperties;

            newCart.transform.parent = cartridgeLocation;
            Debug.Log("currentGame = " + currentGame);
            newCart.parentObject = cartridgeLocation;
            Debug.Log($"position and rotation set. game:{newCart.game}");

            newCart.InsertedIntoPintoBoy(this, trCam2DScene);


            Debug.Log($"about to spawn Networkwide: {newCart.NetworkObject}");

            newCart.NetworkObject.Spawn();
            Debug.Log("currentGame inserted. Removing and Destorying");

            playerHeldBy.DestroyItemInSlotAndSync(slotIndex);
            Destroy(playerHeldBy.ItemSlots[slotIndex]);
            
            Debug.Log("destroyed item in slot");
            if (currentGame != null)
            {
                RemoveCurrentGame(cart);
            }
            currentGame = newCart.game;


        }
        catch (Exception e)
        {
            Debug.Log(e+" Insert Game Failed");
        }
    }

    public void RemoveCurrentGame(PintoBoyCartridge cart)
    {
        Debug.Log("Removing Current Game");
        if(playerHeldBy.FirstEmptyItemSlot() == -1)
        {
            int value = currentGame.cartridge.scrapValue;
            Debug.Log("trying to drop item. Getting parent");
            Transform parent = ((((!(playerHeldBy != null) || !playerHeldBy.isInElevator) && !StartOfRound.Instance.inShipPhase) || !(RoundManager.Instance.spawnedScrapContainer != null)) ? StartOfRound.Instance.elevatorTransform : RoundManager.Instance.spawnedScrapContainer);

            Debug.Log("got parent:"+parent);

            Vector3 vector = base.transform.position + Vector3.up * 0.25f;

            GameObject gameObject = GameObject.Instantiate(Pinto_ModBase.itemLJCartridgePrefab.spawnPrefab, vector, Quaternion.identity, parent);
            GrabbableObject component = gameObject.GetComponent<GrabbableObject>();
            component.startFallingPosition = vector;
            component.targetFloorPosition = component.GetItemFloorPosition(base.transform.position);

            if (playerHeldBy != null && playerHeldBy.isInHangarShipRoom)
            {
                playerHeldBy.SetItemInElevator(droppedInShipRoom: true, droppedInElevator: true, currentGame.cartridge);
            }

            component.SetScrapValue(value);
            component.NetworkObject.Spawn();
        }
        else
        {
            Debug.Log("starting picking up item. Cartridge:"+ cart);
            playerHeldBy.currentlyGrabbingObject = cart;
            playerHeldBy.grabInvalidated = false;
            playerHeldBy.currentlyGrabbingObject.InteractItem();

            Debug.Log("picking up item Interacted with. Cartridge:" + currentGame.cartridge);

            if (!playerHeldBy.isTestingPlayer)
            {
                playerHeldBy.GrabObjectServerRpc(currentGame.cartridge.NetworkObject);
            }
            Debug.Log("picking up item ServerRpc. Cartridge:" + currentGame.cartridge);
            if (playerHeldBy.grabObjectCoroutine != null)
            {
                StopCoroutine(playerHeldBy.grabObjectCoroutine);
            }
            Debug.Log("picking up item Starting Coroutine. Cartridge:" + currentGame.cartridge);
            playerHeldBy.grabObjectCoroutine = StartCoroutine(playerHeldBy.GrabObject());
            Debug.Log("picking up item Coroutine started. Cartridge:" + currentGame.cartridge);
        }
        currentGame = null;
    }

    int FirstItemSlotWithGame()
    {
        if (playerHeldBy == null)
        {
            return -1;
        }

        for(int i=0;i<playerHeldBy.ItemSlots.Length;i++)
        {
            if (playerHeldBy.ItemSlots[i] is PintoBoyCartridge)
            {
                return i;
            }
        }
        return -1;
    }
}