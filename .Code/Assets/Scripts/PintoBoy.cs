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

    bool isButtonPressing = false;      //when the button is being pressed but isnt seen as a hold
    bool isHoldingButton = false;       //when the button is being pressed and it is seen as a hold

    string FadeString = "Fade";
    string DoAnimString = "DoAnim";

    GameObject cam;
    Transform trCam2DScene;
    GameObject modelScreen;
    Transform cartridgeLocation;
    Animator animButton;

    ScanNodeProperties scanNodeProperties;
    public float jumpHeight = 9.75f;
    public float fastFallSpeed = 15f;
    public float rayCastDistance = 0.5f;
    public float rayCastOffset = -0.05f;

    float hideStartHoldTime = 0.2f; //when a button hold is considered started
    float hideStartHoldTimer = 0;

    public bool pressButton = false;

    public NetworkVariable<int> screenId = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    bool spawnScreen = true;

    Material matRenderTex = null;
    RenderTexture texRenderTex = null;

    //public bool isTurnedOff = true;
    public bool isPaused = false;

    Renderer rendModelScreen;

    float batteryDischargeRate = 0.02f;

    float newGameOffset = 20;

    private Coroutine coroutineButtonBeingPressed;


    //isBeingUsed means that the item is on and using battery


    // Start is called before the first frame update
    void Awake()
    {
        audioSource = this.GetComponent<AudioSource>();

        mainObjectRenderer = transform.Find("Model").GetComponent<MeshRenderer>();

        scanNodeProperties = this.GetComponentInChildren<ScanNodeProperties>();
        if (scanNodeProperties == null)
        {
            Debug.Log("scanNodePoperties null af tbh");
        }

        useCooldown = 0.1f;

        grabbable = true;
        parentObject = this.transform;

        Debug.Log("scannode start");
        grabbableToEnemies = true;

        Debug.Log("scannode done");
        modelScreen = transform.Find("Model/Screen").gameObject;
        animButton = transform.Find("Model/Button").GetComponent<Animator>();

        startFallingPosition = new Vector3(0, 0, 0);
        targetFloorPosition = new Vector3(0, 0, 0);

        insertedBattery = new Battery(false, 1);
        EnableItemMeshes(true);

        cartridgeLocation = mainObjectRenderer.transform.Find("Cartridge");

        Debug.Log("cartLoc.childcount:" + cartridgeLocation.childCount);
        if (cartridgeLocation.childCount == 0)
        {
            currentGame = null;
        }

        float rand = Random.value;
        if (rand > 0.5f)
        {

        }
        else
        {

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
            Debug.Log("Error in PintoBoy LateUpdate: " + ex);
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
            cam.GetComponent<Camera>().orthographicSize = 1.5f;


            cam.transform.position = transform.position + (Vector3.down * 400);
            cam.transform.rotation = Quaternion.identity;
            cam.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

            if (cam.transform.position.x == 0 || cam.transform.position.z == 0)
            {
                cam.transform.position = new Vector3(Random.Range(-500f, 500f), cam.transform.position.y, Random.Range(-500f, 500f));
            }

            SetScreenToOffTexture();

            isBeingUsed = true;
            TurnOff();
        }



        if (!isBeingUsed)
        {
            return;
        }
        else
        {
            //insertedBattery.charge -= batteryDischargeRate * Time.deltaTime;
        }
        isHoldingButton = false;

        if (currentGame != null && currentGame.cartridge != null)
        {
            if (isBeingUsed)
            {
                if(isButtonPressing)
                {
                    if(hideStartHoldTimer > 0)
                    {
                        hideStartHoldTimer -= Time.deltaTime;
                    }
                    else
                    {
                        isHoldingButton = true;
                    }
                }
                currentGame.isHoldingButton = this.isHoldingButton;
                InGameUpdate();
            }
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

    void ButtonPress()
    {

        Debug.Log("Button Press PintoBoy. isBeingUsed = " + isBeingUsed);
        if (!isBeingUsed)
        {
            return;
        }

        if (currentGame != null)
        {
            Debug.Log("Button Press PintoBoy. Currentgame = " + currentGame);
            currentGame.ButtonPress();
        }

    }

    public override void UseUpBatteries()
    {
        Debug.Log("battery used up");
        base.UseUpBatteries();

        isBeingUsed = false;
        SetScreenToOffTexture();

        if (currentGame != null)
        {
            currentGame.TurnedOff();
        }

        audioSource.Stop();
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
        if (!isBeingUsed)
        {
            return;
        }

        isPaused = true;
        SetScreenToOffTexture();

        if (currentGame != null)
        {
            currentGame.Pause();
        }

        audioSource.Stop();
    }

    void UnPause()
    {
        if (isBeingUsed)
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
        if (!isBeingUsed)
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
        if (!isBeingUsed)
        {
            return;
        }

        isBeingUsed = false;
        SetScreenToOffTexture();

        if (currentGame != null)
        {
            currentGame.TurnedOff();
        }

        audioSource.Stop();
    }

    void TurnOn()
    {
        if (isBeingUsed)
        {
            return;
        }

        isBeingUsed = true;
        SetScreenToRenderTexture();

        if (currentGame != null)
        {
            currentGame.TurnedOn();
        }
    }

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        Debug.Log("buttondown:" + buttonDown);
        base.ItemActivate(used, buttonDown);
        isButtonPressing = buttonDown;

        if (buttonDown)
        {
            animButton.SetBool("Press", true);
            ButtonPress();
            if (coroutineButtonBeingPressed != null)
            {
                StopCoroutine(coroutineButtonBeingPressed);
            }
            Debug.Log("starting coroutine");
            coroutineButtonBeingPressed = StartCoroutine(ButtonBeingPressed());
        }


        Debug.Log("Pinto Button pressed");
    }

    private IEnumerator ButtonBeingPressed()
    {
        //yield return new WaitForSeconds(0.2f);
        Debug.Log("Pinto Button being pressed" + Time.time);
        float buttonStartPress = Time.time;
        hideStartHoldTimer = hideStartHoldTime;
        Debug.Log("pressed vars: !isHoldingButton:" + !isButtonPressing+", !isHeld:"+!isHeld+", !isBeingUsed:"+!isBeingUsed);
        yield return new WaitUntil(() => !isButtonPressing || !isHeld || !isBeingUsed);
        Debug.Log("Pinto Button end press?" + Time.deltaTime);
        animButton.SetBool("Press", false);
        if(currentGame != null && isBeingUsed)
        {
            currentGame.ButtonRelease(Time.time - buttonStartPress);
        }
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    public void StopSounds()
    {
        audioSource.Stop();
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
        PintoBoy[] boys = (PintoBoy[])FindObjectsByType(typeof(PintoBoy), FindObjectsSortMode.None);
        for (int i = currentId; i < boys.Length + currentId; i++)
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
        if (cam != null)
        {
            return;
        }

        Debug.Log("Spawning Screen");
        cam = transform.Find("2D Cam").gameObject;
        if (cam == null)
        {
            Debug.Log("Screen in Pintoboy is null even after instantiate");
        }

        SetScreenToRenderTexture();

        trCam2DScene = cam.transform.Find("2D Scene");



        Debug.Log("Initializing currentGame");
        if (currentGame != null)
        {
            currentGame.InitializeObjects(cam.transform.Find("2D Scene/Game"));
        }
        else
        {
            Debug.Log("currentGame null when intializing");
        }

        spawnScreen = false;
    }

    void SetScreenToRenderTexture()
    {
        Debug.Log("SetScreenRendTex: checking if texRenderTex null");
        if (texRenderTex == null)
        {

            Debug.Log("SetScreenRendTex: texRenderTex is null. Setting");
            texRenderTex = new RenderTexture(120, 120, 16);

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

            if (currentGame != null)
            {
                currentGame.transform.parent = null;
                //currentGame.Pause();
            }

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


            Debug.Log($"about to spawn Networkwide: {newCart.NetworkObject}");
            newCart.NetworkObject.Spawn();


            newCart.InsertedIntoPintoBoy(this, trCam2DScene);
            Debug.Log("currentGame inserted. Removing and Destorying");

            playerHeldBy.DestroyItemInSlotAndSync(slotIndex);
            Destroy(playerHeldBy.ItemSlots[slotIndex]);


            Debug.Log("destroyed item in slot");
            if (currentGame != null)
            {
                RemoveCurrentGame(cart);

            }
            cam.transform.position += Vector3.forward * newGameOffset;

            currentGame = newCart.game;


            debugCamNum = 0;
        }
        catch (Exception e)
        {
            Debug.Log(e + " Insert Game Failed");
        }
    }

    int debugCamNum = 0;

    public string DebugGetCamChildren()
    {
        string ret = "ret #" + debugCamNum.ToString() + "\n";
        for (int i = 0; i < trCam2DScene.transform.childCount; i++)
        {
            ret += trCam2DScene.transform.GetChild(i) + "\n";
        }
        debugCamNum++;
        return ret;
    }

    public void RemoveCurrentGame(PintoBoyCartridge cart)
    {
        Debug.Log("Removing Current Game");
        if (true)
        {
            int value = currentGame.cartridge.scrapValue;
            Debug.Log("trying to drop item. Getting parent");
            Transform parent = ((((!(playerHeldBy != null) || !playerHeldBy.isInElevator) && !StartOfRound.Instance.inShipPhase) || !(RoundManager.Instance.spawnedScrapContainer != null)) ? StartOfRound.Instance.elevatorTransform : RoundManager.Instance.spawnedScrapContainer);

            Debug.Log("got parent:" + parent);

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
            Debug.Log("starting picking up item. Cartridge:" + cart);
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
        Debug.Log($"2nd this cart name: {cart.game.gameObject.name} cart's parent name: {cart.game.transform.parent}");
        cart.game.TakenOutPintoBoy();
        currentGame = null;
    }

    int FirstItemSlotWithGame()
    {
        if (playerHeldBy == null)
        {
            return -1;
        }

        for (int i = 0; i < playerHeldBy.ItemSlots.Length; i++)
        {
            if (playerHeldBy.ItemSlots[i] is PintoBoyCartridge)
            {
                return i;
            }
        }
        return -1;
    }
}