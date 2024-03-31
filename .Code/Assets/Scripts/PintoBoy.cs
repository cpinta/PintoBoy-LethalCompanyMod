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
using System.Linq;
using System.ComponentModel;
using System.IO;
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

    bool isButtonPressing = false;      //when the button is being pressed but isnt seen as a hold
    protected bool isHoldingButton = false;       //when the button is being pressed and it is seen as a hold

    public GameObject cam;
    Transform trCam2DScene;
    GameObject modelScreen;
    Transform cartridgeLocation;
    Animator animButton;

    ScanNodeProperties scanNodeProperties;

    float hideStartHoldTime = 0.2f; //when a button hold is considered started
    float hideStartHoldTimer = 0;

    public bool pressButton = false;

    bool spawnScreen = true;

    Material matRenderTex = null;
    RenderTexture texRenderTex = null;

    public bool isActive;
    public bool isPaused;
    public bool isInGame;
    public bool wasInitialized = false;

    Renderer rendModelScreen;

    float newGameOffset = 20;

    private Coroutine coroutineButtonBeingPressed;

    public NetworkVariable<float> highScore = new NetworkVariable<float>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> currentScore = new NetworkVariable<float>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> health = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    Color32 bodyColor;
    bool colorSet = false;
    bool testcolor = false;

    //isBeingUsed means that the item is on and using battery


    // Start is called before the first frame update
    protected void PintoAwake()
    {
        audioSource = this.GetComponent<AudioSource>();

        Debug.Log($"PintoBoy Awake");
        propColliders = new Collider[2];
        propColliders[0] = GetComponent<Collider>();
        propColliders[1] = transform.Find("ScanNode").GetComponent<Collider>();
        Debug.Log($"{this.name} propColliders Length:"+propColliders.Length);
        Debug.Log($"{this.name} propColliders Length:" + propColliders.Length + ", item 0:" + propColliders[0]);

        mainObjectRenderer = transform.Find("Model").GetComponent<MeshRenderer>();
        originalScale = new Vector3(0.25f, 0.25f, 0.25f);
        Debug.Log($"{this.name} mainObjectRenderer: {mainObjectRenderer.name}");

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
    }

    // Update is called once per frame
    protected void PintoBoyUpdate()
    {
        base.Update();

        if(IsServer && (!colorSet || testcolor))
        {
            Debug.Log("PintoBoy: setting body color");

            /*Color options:
                White:          #FFFFFF     
                Charcoal:       #575757     
                Red:            #FF6B6B     
                Peach:          #FFDAB9     
                Banana:         #FFFFCC     
                Mint Green:     #98FF98     
                Teal:           #008080     
                Turqoise:       #5999BE
                Light Blue:     #CBCBFF     
                Fairy:          #D9AEDD
             */

            string[] htmlColors = new string[]
             {
                    "#FFFFFF",  // White
                    "#575757",  // Charcoal
                    "#FF6B6B",  // Red
                    "#FFDAB9",  // Peach
                    "#FFFFCC",  // Banana
                    "#98FF98",  // Mint Green
                    "#008080",  // Teal
                    "#5999BE",  // Turquoise
                    "#CBCBFF",  // Light Blue
                    "#D9AEDD"   // Fairy
             };

            Color32[] colorArray = new Color32[htmlColors.Length];
            for (int i = 0; i < htmlColors.Length; i++)
            {
                ColorUtility.TryParseHtmlString(htmlColors[i], out Color color);
                colorArray[i] = color;
            }

            ChangeBodyColorServerRpc(colorArray[Random.Range(0, colorArray.Length)]);
            colorSet = true;
            testcolor = false;
        }

        if (spawnScreen)
        {
            SpawnScreen();
            Debug.Log($"PintoBoy: spawning screen. cam: {cam}");
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
            TurnOff(true);
        }



        if (!isBeingUsed)
        {
            return;
        }

        isHoldingButton = false;

        if (isBeingUsed)
        {
            if (isButtonPressing)
            {
                if (hideStartHoldTimer > 0)
                {
                    hideStartHoldTimer -= Time.deltaTime;
                }
                else
                {
                    isHoldingButton = true;
                }
            }
            GameUpdate();
        }


        //for testing within the Unity Editor
        if (pressButton)
        {
            ButtonPress();
            pressButton = false;
        }
    }

    public virtual void GameUpdate() { }

    public virtual void TurnedOn() { }

    public virtual void InitializeObjects(Transform gameRoot)
    {
        Debug.Log("Intiailizing PintoBoyGame: " + this.name);
        wasInitialized = true;
        gameRoot.localPosition = Vector3.zero;
        gameRoot.localRotation = Quaternion.identity;
        gameRoot.localScale = Vector3.one;
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
            //int firstSlotWGame = FirstItemSlotWithGame();
            //if (firstSlotWGame > -1 && firstSlotWGame < playerHeldBy.ItemSlots.Length)
            //{
            //    InsertGame((PintoBoyCartridge)playerHeldBy.ItemSlots[firstSlotWGame], firstSlotWGame);
            //}
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
        //ChangeOwnershipOfProp(playerHeldBy.playerClientId);
    }

    public virtual void ButtonPress()
    {
    }


    public virtual void ButtonRelease(float timeHeld) 
    { 
    
    }

    public override void UseUpBatteries()
    {
        Debug.Log("PintoBoy battery used up");
        base.UseUpBatteries();

        TurnOff(false);
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

        audioSource.Stop();
    }

    void UnPause()
    {
        if (isBeingUsed)
        {
            return;
        }

        isPaused = false;
    }

    void ToggleOnOff()
    {
        if (!isBeingUsed)
        {
            TurnOn();
        }
        else
        {
            TurnOff(true);
        }
    }

    void TurnOff(bool checkBeingUsed)
    {
        if (!isBeingUsed && checkBeingUsed)
        {
            return;
        }

        isBeingUsed = false;
        SetScreenToOffTexture();

        audioSource.Stop();
        ResetGame();
    }

    void TurnOn()
    {
        if (isBeingUsed)
        {
            return;
        }

        isBeingUsed = true;
        SetScreenToRenderTexture();
    }

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
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
            coroutineButtonBeingPressed = StartCoroutine(ButtonBeingPressed());
        }
    }

    private IEnumerator ButtonBeingPressed()
    {
        //yield return new WaitForSeconds(0.2f);
        Debug.Log("Pinto Button being pressed" + Time.time);
        float buttonStartPress = Time.time;
        yield return new WaitUntil(() => !isButtonPressing || !isHeld);
        Debug.Log("Pinto Button end press " + Time.deltaTime);
        animButton.SetBool("Press", false);
        if(isBeingUsed)
        {
            ButtonRelease(Time.time - buttonStartPress);
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
        Debug.Log("Trying spawn screen");
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
        InitializeObjects(cam.transform.Find("2D Scene/Game"));

        spawnScreen = false;
    }

    private void GetChildRecursive(Transform obj, int level)
    {
        if (null == obj)
            return;

        foreach (Transform child in obj.transform)
        {
            if (null == child)
                continue;
            //child.gameobject contains the current child you can do whatever you want like add it to an array
            Debug.Log(String.Concat(Enumerable.Repeat("*", level)) + obj.name);
            GetChildRecursive(child.transform, level++);
        }
    }

    void SetScreenToRenderTexture()
    {
        Debug.Log("SetScreenRendTex: checking if texRenderTex null");
        if (texRenderTex == null)
        {

            Debug.Log("SetScreenRendTex: texRenderTex is null. Setting. cam:"+cam);
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
        ResetGame();
    }

    public virtual void ResetGame()
    {

    }

    [ServerRpc]
    void ChangeBodyColorServerRpc(Color newColor)
    {
        mainObjectRenderer.materials[0].color = newColor;
        Debug.Log("PintoBoy: server body color to:"+newColor.ToString());
        SetBodyColorClientRpc(newColor.r, newColor.g, newColor.b);
    }


    [ClientRpc]
    void SetBodyColorClientRpc(float r, float g, float b)
    {
        Debug.Log("PintoBoy: client body color");
        //mainObjectRenderer.materials[0].color = new Color(r, g, b);
    }


    //unused method. Works but could cause bugs
    //to make work. Make PintoBoyCartridge into a GrabbableObject and uncomment the lines in this method
    //public void InsertGame(PintoBoyCartridge cart, int slotIndex)
    //{
    //    try
    //    {
    //        Debug.Log("Inserting game: " + cart);

    //        if (currentGame != null)
    //        {
    //            currentGame.transform.parent = null;
    //            //currentGame.Pause();
    //        }

    //        Debug.Log("currentGame = " + currentGame);
    //        PintoBoyCartridge newCart = PintoBoyCartridge.Instantiate(cart, cart.transform.position, cart.transform.rotation, cart.transform.parent);

    //        newCart.game = cart.game;

    //        //newCart.scanNodeProperties = cart.scanNodeProperties;

    //        newCart.transform.parent = cartridgeLocation;
    //        Debug.Log("currentGame = " + currentGame);

    //        //newCart.parentObject = cartridgeLocation;
    //        Debug.Log($"position and rotation set. game:{newCart.game}");

    //        newCart.transform.position = Vector3.zero;
    //        newCart.transform.localRotation = Quaternion.Euler(0, 0, 0);
    //        //newCart.itemProperties.restingRotation = new Vector3(0, 0, 0);

    //        Debug.Log($"about to spawn Networkwide: {newCart.NetworkObject}");
    //        newCart.NetworkObject.Spawn();


    //        newCart.InsertedIntoPintoBoy(this, trCam2DScene);
    //        Debug.Log("currentGame inserted. Removing and Destorying");

    //        playerHeldBy.DestroyItemInSlotAndSync(slotIndex);
    //        Destroy(playerHeldBy.ItemSlots[slotIndex]);


    //        Debug.Log("destroyed item in slot");
    //        if (currentGame != null)
    //        {
    //            RemoveCurrentGame(cart);

    //        }
    //        cam.transform.position += Vector3.forward * newGameOffset;

    //        currentGame = newCart.game;
    //    }
    //    catch (Exception e)
    //    {
    //        Debug.Log(e + " Insert Game Failed");
    //    }
    //}

    //unused method. Works but could cause bugs
    //to make work. Make PintoBoyCartridge into a GrabbableObject and uncomment the lines in this method
    //public void RemoveCurrentGame(PintoBoyCartridge cart)
    //{
    //    Debug.Log("Removing Current Game");
    //    Debug.Log("trying to drop item. Getting parent");
    //    Transform parent = ((((!(playerHeldBy != null) || !playerHeldBy.isInElevator) && !StartOfRound.Instance.inShipPhase) || !(RoundManager.Instance.spawnedScrapContainer != null)) ? StartOfRound.Instance.elevatorTransform : RoundManager.Instance.spawnedScrapContainer);

    //    Debug.Log("got parent:" + parent);

    //    Vector3 vector = base.transform.position + Vector3.up * 0.25f;

    //    GameObject gameObject = GameObject.Instantiate(Pinto_ModBase.itemLJCartridgePrefab.spawnPrefab, vector, Quaternion.identity, parent);
    //    GrabbableObject component = gameObject.GetComponent<GrabbableObject>();
    //    component.startFallingPosition = vector;
    //    component.targetFloorPosition = component.GetItemFloorPosition(base.transform.position);

    //    if (playerHeldBy != null && playerHeldBy.isInHangarShipRoom)
    //    {
    //        //playerHeldBy.SetItemInElevator(droppedInShipRoom: true, droppedInElevator: true, currentGame.cartridge);
    //    }

    //    //component.SetScrapValue(value);
    //    component.NetworkObject.Spawn();
    //    Debug.Log($"2nd this cart name: {cart.game.gameObject.name} cart's parent name: {cart.game.transform.parent}");
    //    cart.game.TakenOutPintoBoy();
    //    currentGame = null;
    //}

    //int FirstItemSlotWithGame()
    //{
    //    if (playerHeldBy == null)
    //    {
    //        return -1;
    //    }

    //    for (int i = 0; i < playerHeldBy.ItemSlots.Length; i++)
    //    {
    //        if (playerHeldBy.ItemSlots[i] is PintoBoyCartridge)
    //        {
    //            return i;
    //        }
    //    }
    //    return -1;
    //}
}