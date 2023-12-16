using LC_API.GameInterfaceAPI;
using LethalLib.Modules;
using PintoMod;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using UnityEngine;

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

public class PintoBoy : PhysicsProp
{
    PintoBoyState gameState = PintoBoyState.MainMenu;

    Animator fadeAnim;
    Transform mainMenu;
    Animator mainMenuAnim;
    float mainmenuTime = 2f;
    float mainmenuTimer = 0f;

    string SelectedString = "Selected";
    string FadeString = "Fade";
    string DoAnimString = "DoAnim";

    Transform inGame;
    GameObject player;
    Rigidbody2D playerRb;
    Collider2D playerCol;
    Collider2D groundCol;
    Animator playerAnim;

    string InAirString = "InAir";

    Transform screen;
    Transform cam;
    ScanNodeProperties scanNodeProperties;
    public float jumpHeight = 17.5f;
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

    public JumpanyEnemy spiderPrefab;
    public JumpanyEnemy lootbugPrefab;
    public JumpanyEnemy slimePrefab;
    float spiderSpeed = 2f;
    float lootbugSpeed = 3f;
    float slimeSpeed = 1.75f;

    float highScore = 0f;
    float currentScore = 0f;
    float scoreIncreaseRate = 15f;     //how much score is added every second

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

    // Start is called before the first frame update
    void Awake()
    {
        fadeAnim = transform.Find("2D Scene/Fade").GetComponent<Animator>();
        mainMenu = transform.Find("2D Scene/Main Menu");
        mainMenuAnim = transform.Find("2D Scene/Main Menu/Main Menu Sprite").GetComponent<Animator>();

        inGame = transform.Find("2D Scene/Game");
        player = transform.Find("2D Scene/Game/PintoEmployee").gameObject;
        playerRb = player.GetComponent<Rigidbody2D>();
        playerAnim = player.GetComponent<Animator>();
        playerCol = player.GetComponent<Collider2D>();
        groundCol = transform.Find("2D Scene/Game/RailingGround").GetComponent<Collider2D>();

        scoreText = transform.Find("2D Scene/Game/UI/Score").GetComponent<TMP_Text>();

        paused = transform.Find("2D Scene/Paused");
        lost = transform.Find("2D Scene/Lost");


        playerRb.bodyType = RigidbodyType2D.Kinematic;

        screen = transform.Find("2D Scene");
        cam = transform.Find("2D Cam");


        scanNodeProperties = this.GetComponentInChildren<ScanNodeProperties>();

        useCooldown = 0.5f;

        grabbable = true;
        parentObject = this.transform;
        //itemProperties = Pinto_ModBase.pintoGrab;

        customGrabTooltip = "Pinto grab tooltup custom";
        grabbableToEnemies = true;


        scanNodeProperties.maxRange = 100;
        scanNodeProperties.minRange = 1;
        scanNodeProperties.requiresLineOfSight = true;
        scanNodeProperties.headerText = "PintoBoy";
        scanNodeProperties.subText = "PintoBoy Subtext";
        scanNodeProperties.creatureScanID = -1;
        scanNodeProperties.nodeType = 2;

        SwitchState(PintoBoyState.MainMenu);
        fadeAnim.gameObject.SetActive(true);

        topSpawnpoint = transform.Find("2D Scene/Game/Top Spawnpoint");
        midSpawnpoint = transform.Find("2D Scene/Game/Mid Spawnpoint");
        bottomSpawnpoint = transform.Find("2D Scene/Game/Bottom Spawnpoint");

        Pinto_ModBase.pintoGrab.canBeGrabbedBeforeGameStart = true;
        Pinto_ModBase.pintoGrab.isScrap = true;
        Pinto_ModBase.pintoGrab.canBeInspected = true;
        Pinto_ModBase.pintoGrab.allowDroppingAheadOfPlayer = true;


        screen.parent = null;
        cam.parent = null;
    }




    // Update is called once per frame
    void Update()
    {
        base.Update();

        if (fadeAnim.GetBool(DoAnimString))
        {
            fadeAnim.SetBool(DoAnimString, false);
        }

        switch (gameState)
        {
            case PintoBoyState.MainMenu:
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
                    SwitchState(PintoBoyState.InGame);
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
        RaycastHit2D hit = Physics2D.Raycast(player.transform.position - (rayCastOffset * Vector3.down), Vector2.down, rayCastDistance);

        if (hit.collider == groundCol)
        {
            isGrounded = true;
            Debug.DrawRay(player.transform.position - (rayCastOffset * Vector3.down), Vector2.down * rayCastDistance, Color.green);
            playerAnim.SetBool(InAirString, false);
        }
        else
        {
            isGrounded = false;
            Debug.DrawRay(player.transform.position - (rayCastOffset * Vector3.down), Vector2.down * rayCastDistance, Color.red);
            playerAnim.SetBool(InAirString, true);
        }

        currentScore += scoreIncreaseRate * Time.deltaTime;

        if (currentScore > speedAdditionMultiplier * increaseAdditionRate && speedAdditionMultiplier > timesIncreased)
        {
            timesIncreased++;
            speedAdditionMultiplier++;
        }

        if (currentScore > lastTimeSpawned + Random.Range(spawnEnemyEveryMin, spawnEnemyEveryMax))
        {
            SpawnRandomEnemy();
            lastTimeSpawned = currentScore;
        }

        scoreText.text = Mathf.Round(currentScore).ToString();
    }

    void SpawnRandomEnemy()
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

    void SpawnEnemy(JumpanyEnemy prefab, Transform position, float speed)
    {
        JumpanyEnemy enemy = Instantiate(prefab, position.position, Quaternion.identity, position);
        enemy.speed = speed + (increaseSpeedAddition * speedAdditionMultiplier);
        enemy.pintoBoy = this;
        enemy.onDeath.AddListener(OnEnemyDeath);
        enemies.Add(enemy);
    }

    void OnEnemyDeath(JumpanyEnemy enemy)
    {
        enemies.Remove(enemy);
    }

    void SpawnSpider()
    {
        SpawnEnemy(spiderPrefab, topSpawnpoint, spiderSpeed);
    }

    void SpawnLootbug()
    {
        SpawnEnemy(lootbugPrefab, midSpawnpoint, lootbugSpeed);
    }

    void SpawnSlime()
    {
        SpawnEnemy(slimePrefab, bottomSpawnpoint, slimeSpeed);
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
        switch (gameState)
        {
            case PintoBoyState.MainMenu:
                mainMenuAnim.SetTrigger(SelectedString);
                mainmenuTimer = mainmenuTime;
                break;
            case PintoBoyState.InGame:
                Jump();
                break;
            case PintoBoyState.Paused:
                break;
            case PintoBoyState.Lost:
                break;
        }
    }

    void StartGame()
    {
        SwitchState(PintoBoyState.InGame);

        playerRb.bodyType = RigidbodyType2D.Dynamic;
        currentScore = 0f;
        speedAdditionMultiplier = speedAdditionMultiplierDefault;

        if (enemies.Count > 0)
        {
            foreach (JumpanyEnemy enemy in enemies)
            {
                Destroy(enemy.gameObject);
            }
            enemies.Clear();
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
        }
        else
        {
            playerRb.velocity = new Vector2(0, -fastFallSpeed);
        }
    }

    private void Enable(bool enable, bool inHand = true)
    {
        //screen.SetActive(enable);
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
        Enable(false, false);
    }

    public float Remap(float from, float fromMin, float fromMax, float toMin, float toMax)
    {
        return 0;
    }

    public void Die()
    {
        if (currentScore > highScore)
        {
            highScore = currentScore;
        }
        StartGame();

    }
}
