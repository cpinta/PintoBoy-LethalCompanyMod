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

public enum PintoBoyState
{
    MainMenu,
    InGame,
    Paused,
    Lost
}

public enum FadeState
{
    FadeOff = 0,
    FadeOn = 1,
    FadeIn = 2,
    FadeOut = 3
}

public class PintoBoy : GrabbableObject
{

    PintoBoyState gameState = PintoBoyState.MainMenu;
    AudioSource audioSource;

    Animator fadeAnim;
    Transform mainMenu;
    Animator mainMenuAnim;
    float mainmenuTime = 2f;
    float mainmenuTimer = 0f;

    float mainmenuWaitTime = 1.5f;
    float mainmenuWaitTimer = 0f;

    string SelectedString = "Selected";
    string FadeString = "Fade";
    string DoAnimString = "DoAnim";

    Transform inGame;
    Vector3 playerStart;
    GameObject player;
    Rigidbody2D playerRb;
    Collider2D playerCol;
    Animator playerAnim;
    SpriteRenderer playerSprite;
    int[] playerPositions = { 0, 5, 10, 15 };

    Vector3 brackenStart;
    GameObject bracken;
    Animator brackenAnim;

    Collider2D groundCol;
    Animator groundAnim;

    string InAirString = "InAir";
    string DeathString = "Death";
    string ResetString = "Reset";

    GameObject cam;
    GameObject modelScreen;
    Animator buttonAnim;

    ScanNodeProperties scanNodeProperties;
    public float jumpHeight = 9.75f;
    public float fastFallSpeed = 15f;
    public float rayCastDistance = 0.5f;
    public float rayCastOffset = -0.05f;

    public bool jump = false;
    bool isGrounded = false;

    bool doOnce = false;

    Transform paused;
    Transform lost;

    Transform playerSpawnpoint;
    Transform topSpawnpoint;
    Transform midSpawnpoint;
    Transform bottomSpawnpoint;

    TMP_Text scoreText;
    TMP_Text endScreenText;

    public GameObject spiderPrefab;
    public GameObject lootbugPrefab;
    public GameObject slimePrefab;
    float spiderSpeed = 2.5f;
    float lootbugSpeed = 3f;
    float slimeSpeed = 2f;

    NetworkVariable<float> highScore = new NetworkVariable<float>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    NetworkVariable<float> currentScore = new NetworkVariable<float>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    float scoreIncreaseRate = 15f;     //how much score is added every second
    NetworkVariable<int> lives = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> screenId = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    float increaseSpeedAddition = 0.25f;    //how much speed is added every increaseAdditionRate score
    float increaseAdditionRate = 100;
    int speedAdditionMultiplier = 1;        //how many times increaseSpeedAddition is added to speed. Increases by 1 every increaseAdditionRate score
    int speedAdditionMultiplierDefault = 1;
    int timesIncreased = 0;

    //float startSpawningScore = 80f;
    float spawnEnemyEveryMin = 40f;
    float spawnEnemyEveryMax = 100f;
    float lastTimeSpawned = 0f;

    List<JumpanyEnemy> enemies = new List<JumpanyEnemy>();

    string camString = "2D Cam/";

    bool invincible = false;
    float invincibleTime = 2f;
    float invincibleTimer = 0f;

    bool dead;
    bool deadHitGround;
    bool deadAnimStarted;
    bool endScreenShown;

    float deathAnimTime = 2.5f;
    float deathAnimTimer = 0f;
    int deathAnimIndex = 0;

    public AudioClip acPlayerStep1;
    public AudioClip acPlayerStep2;
    public AudioClip acPlayerJump;

    public AudioClip acSnapNeck;
    public AudioClip acSnapNeckLand;

    public AudioClip acSpiderStep1;
    public AudioClip acSpiderStep2;
    public AudioClip acSpiderStep3;
    public AudioClip acSpiderStep4;

    public AudioClip acLootbugStep;

    public AudioClip acSlimeStep;

    public AudioClip acConfirm;

    public AudioClip acNewHighscore;
    public AudioClip acNoHighscore;

    public AudioClip acBackgroundSong;

    float stepSoundTimer = 0f;
    float stepSoundTime = 0.25f;
    int stepSoundIndex = 0;

    float backgroundMusicTimer = 0f;
    float backgroundMusicTime = 0f;

    bool firstPlay = true;

    bool spawnScreen = true;

    Material matRenderTex = null;
    RenderTexture texRenderTex = null;

    public bool isTurnedOff = false;
    public bool isPaused = false;

    Renderer rendModelScreen;

    float batteryDischargeRate = 0.002f;

    // Start is called before the first frame update
    void Awake()
    {
        audioSource = this.GetComponent<AudioSource>();

        acPlayerStep1 = Pinto_ModBase.GetAudioClip("21_walk1 (player step 1)");
        acPlayerStep2 = Pinto_ModBase.GetAudioClip("22_walk2 (player step 2)");
        acPlayerJump = Pinto_ModBase.GetAudioClip("23_ladder (player land)");

        acSnapNeck = Pinto_ModBase.GetAudioClip("12_exchange (snap neck)");
        acSnapNeckLand = Pinto_ModBase.GetAudioClip("67_knock (fall after neck snap)");

        acSpiderStep1 = Pinto_ModBase.GetAudioClip("52_step1 (spider movement)");
        acSpiderStep2 = Pinto_ModBase.GetAudioClip("53_step2 (spider movement)");
        acSpiderStep3 = Pinto_ModBase.GetAudioClip("54_step3 (spider movement)");
        acSpiderStep4 = Pinto_ModBase.GetAudioClip("55_step4 (spider movement)");

        acLootbugStep = Pinto_ModBase.GetAudioClip("47_grass (lootbug movement)");

        acSlimeStep = Pinto_ModBase.GetAudioClip("30_triangle (slime movement)");

        acConfirm = Pinto_ModBase.GetAudioClip("59_confirm (start game)");

        acNewHighscore = Pinto_ModBase.GetAudioClip("24_levelclear (new highscore)");
        acNoHighscore = Pinto_ModBase.GetAudioClip("64_lose2 (no highscore)");

        acBackgroundSong = Pinto_ModBase.GetAudioClip("danger_streets");
        backgroundMusicTime = acBackgroundSong.length;

        mainObjectRenderer = transform.Find("Model").GetComponent<MeshRenderer>();

        spiderPrefab = Pinto_ModBase.spiderPrefab;
        lootbugPrefab = Pinto_ModBase.lootbugPrefab;
        slimePrefab = Pinto_ModBase.slimePrefab;

        scanNodeProperties = this.GetComponentInChildren<ScanNodeProperties>();

        useCooldown = 0.1f;

        grabbable = true;
        parentObject = this.transform;
        //customGrabTooltip = "";

        grabbableToEnemies = true;

        scanNodeProperties.maxRange = 100;
        scanNodeProperties.minRange = 1;
        scanNodeProperties.requiresLineOfSight = true;
        scanNodeProperties.headerText = "PintoBoy";
        scanNodeProperties.subText = "PintoBoy Subtext";
        scanNodeProperties.creatureScanID = -1;
        scanNodeProperties.nodeType = 2;



        mainmenuWaitTimer = mainmenuWaitTime;

        modelScreen = transform.Find("Model/Screen").gameObject;
        buttonAnim = transform.Find("Model/Button").GetComponent<Animator>();

        startFallingPosition = new Vector3(0, 0, 0);
        targetFloorPosition = new Vector3(0, 0, 0);

        insertedBattery = new Battery(false, 100);
        EnableItemMeshes(true);
    }

    void LateUpdate()
    {
        try
        {
            base.LateUpdate();
        }
        catch (System.Exception ex)
        {
            Debug.Log("");
            Debug.Log("");
            Debug.Log("");
            Debug.Log("");
            Debug.Log("");
            Debug.Log("Exception caught in PintoBoy LateUpdate: " + ex.Message);
            Debug.Log($"gameState: {gameState}");

            Debug.Log($"fadeAnim: {fadeAnim}");
            Debug.Log($"mainMenu: {mainMenu}");
            Debug.Log($"mainMenuAnim: {mainMenuAnim}");
            Debug.Log($"mainmenuTime: {mainmenuTime}");
            Debug.Log($"mainmenuTimer: {mainmenuTimer}");

            Debug.Log($"SelectedString: {SelectedString}");
            Debug.Log($"FadeString: {FadeString}");
            Debug.Log($"DoAnimString: {DoAnimString}");

            Debug.Log($"inGame: {inGame}");
            Debug.Log($"player: {player}");
            Debug.Log($"playerRb: {playerRb}");
            Debug.Log($"playerCol: {playerCol}");
            Debug.Log($"groundCol: {groundCol}");
            Debug.Log($"playerAnim: {playerAnim}");

            Debug.Log($"InAirString: {InAirString}");

            Debug.Log($"screen: {cam}");
            Debug.Log($"scanNodeProperties: {scanNodeProperties}");
            Debug.Log($"jumpHeight: {jumpHeight}");
            Debug.Log($"fastFallSpeed: {fastFallSpeed}");
            Debug.Log($"rayCastDistance: {rayCastDistance}");
            Debug.Log($"rayCastOffset: {rayCastOffset}");

            Debug.Log($"jump: {jump}");
            Debug.Log($"isGrounded: {isGrounded}");

            Debug.Log($"doOnce: {doOnce}");

            Debug.Log($"paused: {paused}");
            Debug.Log($"lost: {lost}");

            Debug.Log($"playerSpawnpoint: {playerSpawnpoint}");
            Debug.Log($"topSpawnpoint: {topSpawnpoint}");
            Debug.Log($"midSpawnpoint: {midSpawnpoint}");
            Debug.Log($"bottomSpawnpoint: {bottomSpawnpoint}");

            Debug.Log($"scoreText: {scoreText}");

            Debug.Log($"spiderPrefab: {spiderPrefab}");
            Debug.Log($"lootbugPrefab: {lootbugPrefab}");
            Debug.Log($"slimePrefab: {slimePrefab}");
            Debug.Log($"spiderSpeed: {spiderSpeed}");
            Debug.Log($"lootbugSpeed: {lootbugSpeed}");
            Debug.Log($"slimeSpeed: {slimeSpeed}");

            Debug.Log($"highScore: {highScore}");
            Debug.Log($"currentScore: {currentScore}");
            Debug.Log($"scoreIncreaseRate: {scoreIncreaseRate}");

            Debug.Log($"increaseSpeedAddition: {increaseSpeedAddition}");
            Debug.Log($"increaseAdditionRate: {increaseAdditionRate}");
            Debug.Log($"speedAdditionMultiplier: {speedAdditionMultiplier}");
            Debug.Log($"speedAdditionMultiplierDefault: {speedAdditionMultiplierDefault}");
            Debug.Log($"timesIncreased: {timesIncreased}");

            Debug.Log($"spawnEnemyEveryMin: {spawnEnemyEveryMin}");
            Debug.Log($"spawnEnemyEveryMax: {spawnEnemyEveryMax}");
            Debug.Log($"lastTimeSpawned: {lastTimeSpawned}");

            Debug.Log($"enemies Count: {enemies.Count}");
            Debug.Log("");
            Debug.Log("");
            Debug.Log("");
            Debug.Log("");
            Debug.Log("");

        }
    }


    void VerifyScreen()
    {
        if(cam != null)
        {
            Debug.Log("Need to verify screen");
            if (cam.name != "PintoBoy Screen " + screenId.Value.ToString())
            {
                cam = GameObject.Find("PintoBoy Screen " + screenId.Value.ToString());
                if (cam == null)
                {
                    Debug.Log("no cam matching id. Spawning one");
                    SpawnScreen();
                }
                if (!isTurnedOff)
                {
                    Debug.Log("cam found, setting render texture");
                    SetScreenToRenderTexture();
                }
            }
        }
        else
        {
            Debug.Log("verify: cam == null, spawning");
            SpawnScreen();
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
        //VerifyScreen();

        if (isTurnedOff)
        {
            //if(!insertedBattery.empty && isHeld)
            //{
            //    UnPause();
            //}
            return;
        }
        else
        {

            insertedBattery.charge -= batteryDischargeRate * Time.deltaTime;
            //if (!isHeld)
            //{
            //    Pause();
            //}
        }


        if (fadeAnim.GetBool(DoAnimString))
        {
            fadeAnim.SetBool(DoAnimString, false);
        }

        switch (gameState)
        {
            case PintoBoyState.MainMenu:
                if (mainmenuWaitTimer > 0)
                {
                    mainmenuWaitTimer -= Time.deltaTime;
                }

                if (mainmenuTimer > 0)
                {
                    mainmenuTimer -= Time.deltaTime;
                }
                if (mainmenuTimer > 1 && !doOnce)
                {
                    SetFade(FadeState.FadeOut);
                    doOnce = true;
                }
                if (mainmenuTimer <= 1 && mainmenuTimer > 0 && doOnce)
                {
                    mainmenuTimer = 0f;
                    StartGame();
                    SetFade(FadeState.FadeIn);
                    doOnce = false;
                }
                break;
            case PintoBoyState.InGame:
                InGameUpdate();
                break;
            case PintoBoyState.Paused:
                break;
            case PintoBoyState.Lost:
                break;
        }

        if (jump)
        {
            ButtonPress();
            jump = false;
        }
    }

    void InGameUpdate()
    {

        if (endScreenShown)
        {
            return;
        }
        if (!deadAnimStarted)
        {
            RaycastHit2D hit = Physics2D.Raycast(player.transform.position - (rayCastOffset * Vector3.down), Vector2.down, rayCastDistance);

            if (hit.collider == groundCol)
            {
                isGrounded = true;
                Debug.DrawRay(player.transform.position - (rayCastOffset * Vector3.down), Vector2.down * rayCastDistance, Color.green);
                playerAnim.SetBool(InAirString, false);
                deadHitGround = true;
            }
            else
            {
                isGrounded = false;
                Debug.DrawRay(player.transform.position - (rayCastOffset * Vector3.down), Vector2.down * rayCastDistance, Color.red);
                playerAnim.SetBool(InAirString, true);
                deadHitGround = false;
            }
        }

        if (dead)
        {
            if (deadHitGround)
            {
                PlayDeathAnim();
            }
            if (deadAnimStarted)
            {
                deathAnimTimer -= Time.deltaTime;
                if (deathAnimTimer <= 0)
                {
                    deadAnimStarted = false;
                    ShowEndScreen();
                }
                else if (deathAnimTimer < deathAnimTime - 0.25f && deathAnimIndex == 0)
                {
                    deathAnimIndex++;
                    PlaySound(acSnapNeck);
                }
                else if (deathAnimTimer < deathAnimTime - 1.75f && deathAnimIndex == 1)
                {
                    deathAnimIndex++;
                    PlaySound(acSnapNeckLand);
                }
            }


            return;
        }


        if (IsOwner)
        {
            currentScore.Value += scoreIncreaseRate * Time.deltaTime;
        }

        if (currentScore.Value > speedAdditionMultiplier * increaseAdditionRate && speedAdditionMultiplier > timesIncreased)
        {
            timesIncreased++;
            speedAdditionMultiplier++;
        }

        if (IsOwner)
        {
            if (currentScore.Value > lastTimeSpawned + Random.Range(spawnEnemyEveryMin, spawnEnemyEveryMax))
            {
                SpawnRandomEnemyServerRpc();
                lastTimeSpawned = currentScore.Value;
            }
        }

        scoreText.text = Mathf.Round(currentScore.Value).ToString();

        if (player.transform.localPosition.y < playerStart.y -0.5f || player.transform.localPosition.y > playerStart.y + 100)
        {
            player.transform.localPosition = playerSpawnpoint.localPosition;
        }

        if (invincible)
        {
            invincibleTimer -= Time.deltaTime;

            if (invincibleTimer <= 0)
            {
                invincible = false;
                playerSprite.enabled = true;
            }
            else if (invincibleTimer % 0.25f < 0.05f)
            {
                playerSprite.enabled = !playerSprite.enabled;
            }
        }

        if (lives.Value > -1)
        {
            player.transform.localPosition = new Vector3(playerSpawnpoint.localPosition.x + playerPositions[lives.Value], player.transform.localPosition.y, 0);
            bracken.transform.localPosition = new Vector3(playerSpawnpoint.localPosition.x - (playerPositions[lives.Value] * 3), brackenStart.y, 0);
        }
        else
        {
            Debug.Log($"Lives less than 0: {lives}");
        }

        if (isGrounded)
        {
            PlayStepSound();
        }

        if (backgroundMusicTimer > 0)
        {
            backgroundMusicTimer -= Time.deltaTime;
        }
        else
        {
            backgroundMusicTimer = backgroundMusicTime;
            PlaySound(acBackgroundSong);
        }
    }

    [ServerRpc]
    void SpawnRandomEnemyServerRpc()
    {
        int random = Random.Range(0, 3);
        switch (random)
        {
            case 0:
                SpawnSpider();
                break;
            case 1:
                SpawnLootbug();
                break;
            case 2:
                SpawnSlime();
                break;
        }
    }
    public override void ItemInteractLeftRight(bool right)
    {
        base.ItemInteractLeftRight(right);

        if (!right)
        {
            ToggleOnOff();
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

    JumpanyEnemy EnumToJumpanyEnemy(PintoEnemyType enemyType)
    {
        JumpanyEnemy newPrefab = new JumpanyEnemy();
        switch (enemyType)
        {
            case PintoEnemyType.Spider:
                newPrefab = Pinto_ModBase.spiderPrefab.GetComponent<JumpanyEnemy>();
                break;
            case PintoEnemyType.Slime:
                newPrefab = Pinto_ModBase.slimePrefab.GetComponent<JumpanyEnemy>();
                break;
            case PintoEnemyType.Lootbug:
                newPrefab = Pinto_ModBase.lootbugPrefab.GetComponent<JumpanyEnemy>();
                break;
        }
        return newPrefab;
    }

    Transform EnumToEnemySpawnPoint(PintoEnemyType enemyType)
    {
        Transform transform = null;
        switch (enemyType)
        {
            case PintoEnemyType.Spider:
                transform = topSpawnpoint; break;
            case PintoEnemyType.Lootbug:
                transform = midSpawnpoint; break;
            case PintoEnemyType.Slime:
                transform = bottomSpawnpoint; break;
        }
        return transform;
    }

    AudioClip[] EnumToEnemySounds(PintoEnemyType enemyType)
    {
        AudioClip[] sounds = null;
        switch (enemyType)
        {
            case PintoEnemyType.Spider:
                sounds = new AudioClip[] { acSpiderStep1, acSpiderStep2, acSpiderStep3, acSpiderStep4 }; break;
            case PintoEnemyType.Slime:
                sounds = new AudioClip[] { acSlimeStep }; break;
            case PintoEnemyType.Lootbug:
                sounds = new AudioClip[] { acLootbugStep }; break;
        }
        return sounds;
    }

    [ServerRpc]
    void SpawnEnemyServerRpc(float speed, string enemy)
    {
        PintoEnemyType enemyType = Enum.Parse<PintoEnemyType>(enemy);
        JumpanyEnemy newEnemy = SpawnEnemy(EnumToJumpanyEnemy(enemyType), EnumToEnemySpawnPoint(enemyType), speed, enemyType, EnumToEnemySounds(enemyType));
        SpawnEnemyClientRpc(speed, enemy);
    }

    [ClientRpc]
    void SpawnEnemyClientRpc(float speed, string enemy)
    {
        PintoEnemyType enemyType = Enum.Parse<PintoEnemyType>(enemy);

        JumpanyEnemy newEnemy = SpawnEnemy(EnumToJumpanyEnemy(enemyType), EnumToEnemySpawnPoint(enemyType), speed, enemyType, EnumToEnemySounds(enemyType));
    }

    JumpanyEnemy SpawnEnemy(JumpanyEnemy prefab, Transform position, float speed, PintoEnemyType enemy, AudioClip[] audioClips)
    {
        JumpanyEnemy enemyObj = Instantiate(prefab, position.position, Quaternion.identity, position);
        enemyObj.speed = speed + (increaseSpeedAddition * speedAdditionMultiplier);
        enemyObj.pintoBoy = this;
        enemyObj.onDeath.AddListener(OnEnemyDeath);
        enemyObj.enemyType = enemy;
        enemyObj.SetMovementSounds(audioClips);
        enemies.Add(enemyObj);
        return enemyObj;
    }

    void OnEnemyDeath(JumpanyEnemy enemy)
    {
        enemies.Remove(enemy);
        enemy.onDeath.RemoveListener(OnEnemyDeath);
        Destroy(enemy.gameObject);
    }

    void SpawnSpider()
    {
        SpawnEnemyServerRpc(spiderSpeed, PintoEnemyType.Spider.ToString());
    }

    void SpawnLootbug()
    {
        SpawnEnemyServerRpc(lootbugSpeed, PintoEnemyType.Lootbug.ToString());
    }

    void SpawnSlime()
    {
        SpawnEnemyServerRpc(slimeSpeed, PintoEnemyType.Slime.ToString());
    }

    void SetFade(FadeState state)
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
        buttonAnim.SetTrigger("Press");
        if (isTurnedOff)
        {
            return;
        }
        switch (gameState)
        {
            case PintoBoyState.MainMenu:
                if (mainmenuWaitTimer <= 0 && mainmenuTimer <= 0)
                {
                    StartFromTitleScreen();
                }
                else
                {
                    return;
                }
                break;
            case PintoBoyState.InGame:
                if (dead)
                {
                    if (!endScreenShown)
                    {
                        ShowEndScreen();
                    }
                    else
                    {
                        ShowTitleScreen();
                    }
                    return;
                }
                Jump();
                break;
            case PintoBoyState.Paused:
                break;
            case PintoBoyState.Lost:
                break;
        }
    }

    void StartFromTitleScreen()
    {
        mainMenuAnim.SetTrigger(SelectedString);
        mainmenuTimer = mainmenuTime;
        PlaySound(acConfirm);
    }

    void ShowTitleScreen()
    {
        SwitchState(PintoBoyState.MainMenu);
        mainmenuWaitTimer = mainmenuWaitTime;
    }

    [ServerRpc]
    void StartGameServerRpc()
    {
        StartGame();
        StartGameClientRpc();
    }

    [ClientRpc]
    void StartGameClientRpc()
    {
        StartGame();
    }

    void StartGame()
    {

        SwitchState(PintoBoyState.InGame);

        playerRb.bodyType = RigidbodyType2D.Dynamic;
        speedAdditionMultiplier = speedAdditionMultiplierDefault;

        brackenAnim.SetTrigger(ResetString);
        playerAnim.SetTrigger(ResetString);

        ClearEnemies();

        groundAnim.enabled = true;
        dead = false;
        endScreenText.text = "";
        deadAnimStarted = false;
        endScreenShown = false;
        deathAnimTimer = 0f;

        if (IsOwner)
        {
            lives.Value = 3;
            currentScore.Value = 0f;
        }

        timesIncreased = 0;
        lastTimeSpawned = 0f;
        deathAnimIndex = 0;

        firstPlay = false;
        backgroundMusicTimer = 0;
    }

    void ClearEnemies()
    {
        if (enemies.Count > 0)
        {
            foreach (JumpanyEnemy enemy in enemies)
            {
                Destroy(enemy.gameObject);
            }
            enemies.Clear();
        }
    }

    void ClearEnemiesExcept(JumpanyEnemy enemy)
    {
        if (enemies.Count > 0)
        {
            foreach (JumpanyEnemy enemys in enemies)
            {
                if (enemys == enemy)
                {
                    continue;
                }
                Destroy(enemys.gameObject);
                enemies.Remove(enemys);
            }
        }
    }

    [ServerRpc]
    void SwitchStateServerRpc(PintoBoyState newState)
    {
        SwitchState(newState);
        SwitchStateClientRpc(newState);
    }

    [ClientRpc]
    void SwitchStateClientRpc(PintoBoyState newState)
    {
        SwitchState(newState);
    }

    void SwitchState(PintoBoyState newState)
    {

        switch (newState)
        {
            case PintoBoyState.MainMenu:
                mainMenu.gameObject.SetActive(true);
                inGame.gameObject.SetActive(false);
                paused.gameObject.SetActive(false);
                lost.gameObject.SetActive(false);
                doOnce = false;
                break;
            case PintoBoyState.InGame:
                mainMenu.gameObject.SetActive(false);
                inGame.gameObject.SetActive(true);
                paused.gameObject.SetActive(false);
                lost.gameObject.SetActive(false);
                break;
            case PintoBoyState.Paused:
                mainMenu.gameObject.SetActive(false);
                inGame.gameObject.SetActive(false);
                paused.gameObject.SetActive(true);
                lost.gameObject.SetActive(false);
                break;
            case PintoBoyState.Lost:
                mainMenu.gameObject.SetActive(false);
                inGame.gameObject.SetActive(false);
                paused.gameObject.SetActive(false);
                lost.gameObject.SetActive(true);
                break;
        }
        gameState = newState;
    }

    void Jump()
    {
        if (isGrounded)
        {
            playerRb.velocity = new Vector2(0, jumpHeight);
            PlaySound(acPlayerJump);
        }
        else
        {
            playerRb.velocity = new Vector2(0, -fastFallSpeed);
            PlaySound(acPlayerJump);
        }
    }

    void TogglePause()
    {
        if (isTurnedOff)
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
        DisableAllAnimators();
        audioSource.Stop();
    }

    void UnPause()
    {
        if(!isTurnedOff)
        {
            return;
        }

        isPaused = false;
        //Insert Pause Menu enable here
        EnableAllAnimators();
        if(gameState == PintoBoyState.InGame && !dead)
        {
            audioSource.PlayOneShot(acBackgroundSong);
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

    void TurnOn()
    {
        if (isTurnedOff)
        {
            return;
        }

        isTurnedOff = true;
        SetScreenToOffTexture();
        DisableAllAnimators();
        audioSource.Stop();
    }

    void TurnOff()
    {
        if (!isTurnedOff)
        {
            return;
        }

        isTurnedOff = false;
        SetScreenToRenderTexture();
        EnableAllAnimators();

        ClearEnemies();
        ShowTitleScreen();
    }

    void EnableAllAnimators()
    {
        groundAnim.enabled = true;
        brackenAnim.enabled = true;
        mainMenuAnim.enabled = true;
        playerAnim.enabled = true;
        fadeAnim.enabled = true;

        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].animator.enabled = true;
            enemies[i].paused = true;
        }
    }

    void DisableAllAnimators()
    {
        groundAnim.enabled = false;
        brackenAnim.enabled = false;
        mainMenuAnim.enabled = false;
        playerAnim.enabled = false;
        fadeAnim.enabled = false;

        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].animator.enabled = false;
            enemies[i].paused = false;
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
        Pause();
    }

    public float Remap(float from, float fromMin, float fromMax, float toMin, float toMax)
    {
        return 0;
    }


    public void PlayerGotHit()
    {
        if (!IsOwner)
        {
            return;
        }

        if (invincible)
        {
            return;
        }


        lives.Value--;

        if (lives.Value <= 0)
        {
            DieServerRpc();
        }
        else
        {
            invincible = true;
            invincibleTimer = invincibleTime;
        }
    }

    [ServerRpc]
    public void DieServerRpc()
    {
        Die();
        DieClientRpc();
    }

    [ClientRpc]
    public void DieClientRpc()
    {
        Die();
    }

    public void Die()
    {
        groundAnim.enabled = false;
        dead = true;

        for(int i = 0; i<enemies.Count; i++)
        {
            enemies[i].killedPlayer = true;
        }
        if (Mathf.Abs(player.transform.localPosition.x - playerSpawnpoint.localPosition.x) < 5)
        {
            playerRb.velocity = new Vector2(0, 0);
            player.transform.localPosition = new Vector3(player.transform.localPosition.x, playerSpawnpoint.localPosition.y, 0);
        }
    }

    public void PlayDeathAnim()
    {
        player.transform.localPosition = playerSpawnpoint.localPosition;
        brackenAnim.SetTrigger(DeathString);
        playerAnim.SetTrigger(DeathString);
        deathAnimTimer = deathAnimTime;
        deadAnimStarted = true;
        deadHitGround = false;
        deathAnimIndex = 0;
        audioSource.Stop();
    }

    void ShowEndScreen()
    {
        if (currentScore.Value > highScore.Value)
        {
            endScreenText.text = $"New Best!\n" +
                                 $"{Mathf.Round(currentScore.Value)}\n" +
                                 $"Last Best:\n" +
                                 $"{Mathf.Round(highScore.Value)}";
            if (IsOwner)
            {
                highScore.Value = currentScore.Value;
            }
            PlaySound(acNewHighscore);
        }
        else
        {
            endScreenText.text = $"Score:\n" +
                                 $"{Mathf.Round(currentScore.Value)}\n" +
                                 $"Best:\n" +
                                 $"{Mathf.Round(highScore.Value)}";
            PlaySound(acNoHighscore);
        }
        endScreenShown = true;
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    void PlayStepSound()
    {
        stepSoundTimer -= Time.deltaTime;
        if (stepSoundTimer <= 0)
        {
            stepSoundTimer = stepSoundTime;
            stepSoundIndex++;
            if (stepSoundIndex > 1)
            {
                stepSoundIndex = 0;
            }
            switch (stepSoundIndex)
            {
                case 0:
                    PlaySound(acPlayerStep1);
                    break;
                case 1:
                    PlaySound(acPlayerStep2);
                    break;
            }
        }
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


        fadeAnim = cam.transform.Find("2D Scene/Fade").GetComponent<Animator>();

        mainMenu = cam.transform.Find("2D Scene/Main Menu");
        mainMenuAnim = cam.transform.Find("2D Scene/Main Menu/Main Menu Sprite").GetComponent<Animator>();

        inGame = cam.transform.Find("2D Scene/Game");
        player = cam.transform.Find("2D Scene/Game/PintoEmployee").gameObject;
        playerStart = player.transform.localPosition;
        playerRb = player.GetComponent<Rigidbody2D>();
        playerAnim = player.GetComponent<Animator>();
        playerCol = player.GetComponent<Collider2D>();
        playerSprite = player.GetComponent<SpriteRenderer>();

        bracken = cam.transform.Find("2D Scene/Game/Bracken").gameObject;
        brackenAnim = bracken.GetComponent<Animator>();
        brackenStart = bracken.transform.localPosition;

        groundCol = cam.transform.Find("2D Scene/Game/RailingGround").GetComponent<Collider2D>();
        groundAnim = groundCol.gameObject.GetComponent<Animator>();

        scoreText = cam.transform.Find("2D Scene/Game/UI/Score").GetComponent<TMP_Text>();
        endScreenText = cam.transform.Find("2D Scene/Game/UI/Death Screen/Text").GetComponent<TMP_Text>();

        paused = cam.transform.Find("2D Scene/Paused");
        lost = cam.transform.Find("2D Scene/Lost");

        playerRb.bodyType = RigidbodyType2D.Kinematic;

        topSpawnpoint = cam.transform.Find("2D Scene/Game/Top Spawnpoint");
        midSpawnpoint = cam.transform.Find("2D Scene/Game/Mid Spawnpoint");
        bottomSpawnpoint = cam.transform.Find("2D Scene/Game/Bottom Spawnpoint");
        playerSpawnpoint = cam.transform.Find("2D Scene/Game/Player Spawnpoint");



        SwitchState(PintoBoyState.MainMenu);

        fadeAnim.gameObject.SetActive(true);
        endScreenText.text = "";

        spawnScreen = false;
    }

    //[ClientRpc]
    //void SpawnCameraClientRpc(Transform camera)
    //{
    //    if (camera == null)
    //    {
    //        return;
    //    }

    //    cam = camera;
    //}

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
}