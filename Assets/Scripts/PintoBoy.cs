//using LC_API.GameInterfaceAPI;
//using LethalLib.Modules;
//using PintoMod;
using PintoMod;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine.UIElements.UIR;

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

    Transform cam;
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

    float highScore = 0f;
    float currentScore = 0f;
    float scoreIncreaseRate = 15f;     //how much score is added every second
    int lives = 3;

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




    // Update is called once per frame
    void Update()
    {
        base.Update();

        if (spawnScreen)
        {
            SpawnScreen();
            spawnScreen = false;
        }

        if(isPaused)
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



        currentScore += scoreIncreaseRate * Time.deltaTime;

        if (currentScore > speedAdditionMultiplier * increaseAdditionRate && speedAdditionMultiplier > timesIncreased)
        {
            timesIncreased++;
            speedAdditionMultiplier++;
        }

        if (IsOwner)
        {
            if (currentScore > lastTimeSpawned + Random.Range(spawnEnemyEveryMin, spawnEnemyEveryMax))
            {
                SpawnRandomEnemyServerRpc();
                lastTimeSpawned = currentScore;
            }
        }

        scoreText.text = Mathf.Round(currentScore).ToString();

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

        if (lives > -1)
        {
            player.transform.localPosition = new Vector3(playerSpawnpoint.localPosition.x + playerPositions[lives], player.transform.localPosition.y, 0);
            bracken.transform.localPosition = new Vector3(playerSpawnpoint.localPosition.x - (playerPositions[lives] * 3), brackenStart.y, 0);
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
            TogglePause();
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
    }

    JumpanyEnemy SpawnEnemy(JumpanyEnemy prefab, Transform position, float speed, PintoEnemyType enemy, AudioClip[] audioClips)
    {
        JumpanyEnemy enemyObj = Instantiate(prefab, position.position, Quaternion.identity, position);
        enemyObj.speed = speed + (increaseSpeedAddition * speedAdditionMultiplier);
        //enemyObj.pintoBoy = this;
        enemyObj.onDeath.AddListener(OnEnemyDeath);
        enemyObj.enemyType = enemy;
        enemyObj.SetMovementSounds(audioClips);
        enemies.Add(enemyObj);
        NetworkObject netobj = enemyObj.GetComponent<NetworkObject>();
        netobj.Spawn();
        netobj.TrySetParent(transform.root, false);
        netobj.transform.SetPositionAndRotation(transform.position, transform.rotation);
        
        //enemyObj.transform.position = position.position;
        //enemyObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
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
        JumpanyEnemy spider = SpawnEnemy(spiderPrefab.GetComponent<JumpanyEnemy>(), topSpawnpoint, spiderSpeed, PintoEnemyType.Spider, new AudioClip[] { acSpiderStep1, acSpiderStep2, acSpiderStep3, acSpiderStep4 });
    }

    void SpawnLootbug()
    {
        JumpanyEnemy lootbug = SpawnEnemy(lootbugPrefab.GetComponent<JumpanyEnemy>(), midSpawnpoint, lootbugSpeed, PintoEnemyType.Lootbug, new AudioClip[] { acLootbugStep });
    }

    void SpawnSlime()
    {
        JumpanyEnemy slime = SpawnEnemy(slimePrefab.GetComponent<JumpanyEnemy>(), bottomSpawnpoint, slimeSpeed, PintoEnemyType.Slime, new AudioClip[] { acSlimeStep });
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
        if (isPaused)
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

    void StartGame()
    {
        SwitchState(PintoBoyState.InGame);

        playerRb.bodyType = RigidbodyType2D.Dynamic;
        currentScore = 0f;
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

        lives = 3;

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
        if (isPaused)
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
        if(!isPaused)
        {
            return;
        }

        isPaused = false;
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

    public void PlayerGotHit(JumpanyEnemy enemy)
    {
        if (invincible)
        {
            return;
        }
        lives--;
        if (lives <= 0)
        {
            Die(enemy);
        }
        else
        {
            invincible = true;
            invincibleTimer = invincibleTime;
        }
    }

    public void Die(JumpanyEnemy enemy)
    {
        ClearEnemiesExcept(enemy);
        groundAnim.enabled = false;
        dead = true;
        enemy.killedPlayer = true;
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
        if (currentScore > highScore)
        {
            endScreenText.text = $"New Best!\n" +
                                 $"{Mathf.Round(currentScore)}\n" +
                                 $"Last Best:\n" +
                                 $"{Mathf.Round(highScore)}";
            highScore = currentScore;
            PlaySound(acNewHighscore);
        }
        else
        {
            endScreenText.text = $"Score:\n" +
                                 $"{Mathf.Round(currentScore)}\n" +
                                 $"Best:\n" +
                                 $"{Mathf.Round(highScore)}";
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

    void SpawnScreen()
    {
        if(Pinto_ModBase.screenPrefab == null)
        {
            Debug.Log("Screen is null");
        }
        cam = Instantiate(Pinto_ModBase.screenPrefab, this.transform.position + (Vector3.down * 300), Quaternion.identity, transform.root).transform;
        if(cam == null)
        {
            Debug.Log("Screen in Pintoboy is null");
        }

        SetScreenToRenderTexture();

        cam.parent = null;

        fadeAnim = cam.Find("2D Scene/Fade").GetComponent<Animator>();

        mainMenu = cam.Find("2D Scene/Main Menu");
        mainMenuAnim = cam.Find("2D Scene/Main Menu/Main Menu Sprite").GetComponent<Animator>();

        inGame = cam.Find("2D Scene/Game");
        player = cam.Find("2D Scene/Game/PintoEmployee").gameObject;
        playerStart = player.transform.localPosition;
        playerRb = player.GetComponent<Rigidbody2D>();
        playerAnim = player.GetComponent<Animator>();
        playerCol = player.GetComponent<Collider2D>();
        playerSprite = player.GetComponent<SpriteRenderer>();

        bracken = cam.Find("2D Scene/Game/Bracken").gameObject;
        brackenAnim = bracken.GetComponent<Animator>();
        brackenStart = bracken.transform.localPosition;

        groundCol = cam.Find("2D Scene/Game/RailingGround").GetComponent<Collider2D>();
        groundAnim = groundCol.gameObject.GetComponent<Animator>();

        scoreText = cam.Find("2D Scene/Game/UI/Score").GetComponent<TMP_Text>();
        endScreenText = cam.Find("2D Scene/Game/UI/Death Screen/Text").GetComponent<TMP_Text>();

        paused = cam.Find("2D Scene/Paused");
        lost = cam.Find("2D Scene/Lost");

        playerRb.bodyType = RigidbodyType2D.Kinematic;

        topSpawnpoint = cam.Find("2D Scene/Game/Top Spawnpoint");
        midSpawnpoint = cam.Find("2D Scene/Game/Mid Spawnpoint");
        bottomSpawnpoint = cam.Find("2D Scene/Game/Bottom Spawnpoint");
        playerSpawnpoint = cam.Find("2D Scene/Game/Player Spawnpoint");


        SwitchState(PintoBoyState.MainMenu);

        fadeAnim.gameObject.SetActive(true);
        endScreenText.text = "";

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
}