using PintoMod.Assets.Scripts;
using PintoMod.Assets.Scripts.LethalJumpany;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PintoMod.Assets.Scripts.FacilityDash
{

    public enum FDState
    {
        MainMenu,
        InGame,
        Lost
    }

    public enum FDPlayerState
    {
        Idle,
        Walking,
        Hiding,
        Latched,
        Dead,
        Done        //used for when between levels
    }

    public enum FDHallwayState
    {
        Enter = -1,
        Inside = 0,
        Exit = 1,
    }

    public enum FDEnemyType
    {
        Bracken = 0,
        BunkerSpider = 1,
        LootBug = 2,
        Nutcracker = 3,
        SnareFlea = 4,
        Thumper = 5,
    }

    public class FD_EnemyWeight
    {
        public FD_Enemy enemy;
        public int weight;
        public FDEnemyType type;

        public FD_EnemyWeight(FD_Enemy enemy, int weight, FDEnemyType type)
        {
            this.enemy = enemy;
            this.weight = weight;
            this.type = type;
        }
    }

    public class FD_Level
    {
        public int levelIndex;
        public float speed;
        public int distanceLength;
        public float monsterSpawnChance;
        public List<FD_EnemyWeight> enemyList;

        public FD_Level(int levelIndex, float speed, int distanceLength, float monsterSpawnChance, List<FD_EnemyWeight> enemyList)
        {
            this.levelIndex = levelIndex;
            this.speed = speed;
            this.distanceLength = distanceLength;
            this.monsterSpawnChance = monsterSpawnChance;
            this.enemyList = enemyList;
        }
    }

    public class FacilityDash : PintoBoy
    {
        public FDState gameState;
        FDPlayerState _playerState;
        public FDPlayerState playerState { get => _playerState; set { _playerState = value; } }
        public FDHallwayState hallwayState;
        public bool pressButton = false;
        public bool hideButton = false;

        public Transform trGameRoot;
        Transform trInGame;

        Animator animFade;

        Transform trMainMenu;
        Animator animMainMenu;
        float mainmenuWaitTime = 1.5f;
        float mainmenuWaitTimer = 0f;

        Animator animGame;
        string strHidingString = "Hiding";
        public bool isHiding = false;
        float postHidingTime = 0.2f;        //the cooldown time between unhiding and being able to attack
        float postHidingTimer = 0;          //the timer for that ^^^
        Animator animHallway;
        string strStateString = "State";
        string strEnterExitString = "EnterExit";
        int hallwayFrameCount = 6;
        int hallwayMonsterSpawnFrame = 3;
        int hallwayEnterFrameCount = 8;
        int hallwayExitFrameCount = 10;

        float startWaitTimer = 0;
        float startWaitTime = 2;

        //Enemy
        Transform trEnemySpawn;
        FD_Enemy currentEnemy;
        bool isEnemyInFront = false;
        List<FD_EnemyWeight> currentEnemyList = new List<FD_EnemyWeight>();
        Dictionary<string, int> enemyTimesEncountered = new Dictionary<string, int>();
        int totalWeight;

        bool hasEnemyBeenRolled = false;
        float currentMonsterSpawnChance = .2f;

        public GameObject prefabSnareFlea;
        public GameObject prefabThumper;
        public GameObject prefabBunkerSpider;
        public GameObject prefabLootBug;
        public GameObject prefabNutcracker;
        public GameObject prefabBracken;

        //Levels
        int levelIndex = 0;
        List<FD_Level> levels = new List<FD_Level>();

        //Shovel
        Transform trShovelSpawnpoint;
        Transform trShovel;
        Animator animShovel;
        FD_Shovel shovel;
        string strIsWalkingString = "isWalking";
        string strHitString = "Hit";
        string strGotHitString = "GotHit";
        string strDeadString = "Dead";
        string strBobString = "Bob";
        int shovelWalkYOffset = 0;

        //UI
        Transform trUI;
        TMP_Text txtScore;
        TMP_Text txtEndScreen;
        Animator animPlayerHealth;
        Animator animPlayerStamina;
        Animator animButtonTooltips;


        TMP_Text txtLevelEndScreen;
        float levelTransitionTime = 6;      //total time for level transition. The text changes at levelTransitionTime/2
        float levelTransitionTimer = 0;
        int levelTransitionPhase = 0;       //the level transition shows "Level 1 Complete" (this = 0) then shows "Level 2 Starting" (this = 1)

        //Gameplay Variables
        int lives = 1;
        int basePlayerHealth = 4;
        int playerHealth = 0;
        float basePlayerStamina = 4;
        float playerStamina = 0;
        float staminaLossPerSec = 1;
        int distanceMod = 0;
        int oldDistanceMod = 0;
        int highscore = 0;
        float currentFrameDistance = 0;     //gameSpeed * Time.deltaTime
        //float distanceTraveled = 0;         //distanceTraveled this level
        float previousDistanceTraveled = 0; //distanceTraveled within the last levels combined
        float currentLevelDistanceGoal = 0; //distance needed to move on to the next level
        public float startingGameSpeed = 4;
        public float gameSpeed = 4;
        float gameSpeedIncrement = 1;

        float damageOverTimeTimer = 0;
        float damageOverTimeTime = 2;

        //Sound
        AudioClip acNewHighscore;
        AudioClip acNoHighscore;

        AudioClip acPlayerAttack;
        AudioClip acPlayerAttack2;
        AudioClip acFacilityCleared;
        AudioClip acEnemyKilled;

        AudioClip acSong1;
        AudioClip acSong2;

        AudioClip acSnapNeck;


        private void Awake()
        {
            PintoAwake();

            Debug.Log("PintoBoy FD: FD Awake");
            mainmenuWaitTimer = mainmenuWaitTime;

            acNewHighscore = Pinto_ModBase.GetAudioClip(Pinto_ModBase.fdAudioPath + "80_chaindone (highscore)");
            acNoHighscore = Pinto_ModBase.GetAudioClip(Pinto_ModBase.fdAudioPath + "37_level (no highscore)");

            acPlayerAttack = Pinto_ModBase.GetAudioClip(Pinto_ModBase.fdAudioPath + "12_exchange (player attack)");
            acPlayerAttack2 = Pinto_ModBase.GetAudioClip(Pinto_ModBase.fdAudioPath + "78_arrowhit (player attack 2)");
            acFacilityCleared = Pinto_ModBase.GetAudioClip(Pinto_ModBase.fdAudioPath + "28_jingle (Facility Cleared)");
            acEnemyKilled = Pinto_ModBase.GetAudioClip(Pinto_ModBase.fdAudioPath + "57_drop (Enemy killed)");

            acSong1 = Pinto_ModBase.GetAudioClip(Pinto_ModBase.fdAudioPath + "encounter");
            acSong2 = Pinto_ModBase.GetAudioClip(Pinto_ModBase.fdAudioPath + "intruder_alert");

            acSnapNeck = Pinto_ModBase.GetAudioClip(Pinto_ModBase.fdAudioPath + "monster sounds/Bracken snap neck");

            Debug.Log("PintoBoy FD: Loaded Audio.");

            //prefabBracken = Pinto_ModBase.fdBrackenPrefab;
            //prefabBunkerSpider = Pinto_ModBase.fdBunkerSpiderPrefab;
            //prefabLootBug = Pinto_ModBase.fdLootBugPrefab;
            //prefabNutcracker = Pinto_ModBase.fdNutcrackerPrefab;
            //prefabSnareFlea = Pinto_ModBase.fdSnareFleaPrefab;
            //prefabThumper = Pinto_ModBase.fdThumperPrefab;

            gameState = FDState.MainMenu;

            List<FD_EnemyWeight> enemyList = new List<FD_EnemyWeight>();
            enemyList.Add(new FD_EnemyWeight(prefabThumper.GetComponent<FD_Enemy>(), 100, FDEnemyType.Thumper));
            enemyList.Add(new FD_EnemyWeight(prefabBunkerSpider.GetComponent<FD_Enemy>(), 30, FDEnemyType.BunkerSpider));
            enemyList.Add(new FD_EnemyWeight(prefabLootBug.GetComponent<FD_Enemy>(), 60, FDEnemyType.LootBug));
            //enemyList.Add(new FD_EnemyWeight(prefabBracken, 500000000));

            int lvlIndex = 0;
            levels.Add(new FD_Level(lvlIndex, 5, 20, 1, enemyList.ToList()));
            lvlIndex++;
            enemyList.Clear();

            enemyList.Add(new FD_EnemyWeight(prefabThumper.GetComponent<FD_Enemy>(), 90, FDEnemyType.Thumper));
            enemyList.Add(new FD_EnemyWeight(prefabBunkerSpider.GetComponent<FD_Enemy>(), 40, FDEnemyType.BunkerSpider));
            enemyList.Add(new FD_EnemyWeight(prefabLootBug.GetComponent<FD_Enemy>(), 60, FDEnemyType.LootBug));
            enemyList.Add(new FD_EnemyWeight(prefabSnareFlea.GetComponent<FD_Enemy>(), 10, FDEnemyType.SnareFlea));

            levels.Add(new FD_Level(lvlIndex, 5.5f, 50, .75f, enemyList.ToList()));
            lvlIndex++;
            enemyList.Clear();

            enemyList.Add(new FD_EnemyWeight(prefabThumper.GetComponent<FD_Enemy>(), 90, FDEnemyType.Thumper));
            enemyList.Add(new FD_EnemyWeight(prefabBunkerSpider.GetComponent<FD_Enemy>(), 40, FDEnemyType.BunkerSpider));
            enemyList.Add(new FD_EnemyWeight(prefabLootBug.GetComponent<FD_Enemy>(), 60, FDEnemyType.LootBug));
            enemyList.Add(new FD_EnemyWeight(prefabSnareFlea.GetComponent<FD_Enemy>(), 40, FDEnemyType.SnareFlea));
            enemyList.Add(new FD_EnemyWeight(prefabBracken.GetComponent<FD_Enemy>(), 10, FDEnemyType.Bracken));

            levels.Add(new FD_Level(lvlIndex, 6f, 60, .75f, enemyList.ToList()));
            lvlIndex++;
            enemyList.Clear();

            enemyList.Add(new FD_EnemyWeight(prefabThumper.GetComponent<FD_Enemy>(), 90, FDEnemyType.Thumper));
            enemyList.Add(new FD_EnemyWeight(prefabBunkerSpider.GetComponent<FD_Enemy>(), 40, FDEnemyType.BunkerSpider));
            enemyList.Add(new FD_EnemyWeight(prefabLootBug.GetComponent<FD_Enemy>(), 60, FDEnemyType.LootBug));
            enemyList.Add(new FD_EnemyWeight(prefabSnareFlea.GetComponent<FD_Enemy>(), 40, FDEnemyType.SnareFlea));
            enemyList.Add(new FD_EnemyWeight(prefabBracken.GetComponent<FD_Enemy>(), 20, FDEnemyType.Bracken));

            levels.Add(new FD_Level(lvlIndex, 6.5f, 80, .75f, enemyList.ToList()));
            lvlIndex++;
            enemyList.Clear();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            PintoBoyUpdate();
            if (pressButton)
            {
                ButtonPress();
                pressButton = false;
            }

            if (startWaitTimer > 0)
            {
                startWaitTimer -= Time.deltaTime;
            }
        }

        void Hide()
        {
            if (playerState == FDPlayerState.Idle || playerState == FDPlayerState.Hiding)
            {
                if (playerStamina > 0 && !isHiding)
                {
                    animGame.SetBool(strHidingString, true);
                    playerState = FDPlayerState.Hiding;
                    isHiding = true;
                }
            }
        }

        void UnHide()
        {
            if (playerState == FDPlayerState.Idle || playerState == FDPlayerState.Hiding)
            {
                if (isHiding)
                {
                    animGame.SetBool(strHidingString, false);
                    if (currentEnemy != null)
                    {
                        playerState = FDPlayerState.Idle;
                    }
                    else
                    {
                        playerState = FDPlayerState.Walking;
                    }
                    postHidingTimer = postHidingTime;
                    isHiding = false;
                }
            }
        }

        public override void ButtonPress()
        {
            base.ButtonPress();
        }

        public override void ButtonRelease(float timeHeld)
        {
            base.ButtonRelease(timeHeld);

            switch (gameState)
            {
                case FDState.MainMenu:
                    if (startWaitTimer > 0)
                    {
                        break;
                    }
                    PlaySound(acPlayerAttack);
                    StartGame();
                    animMainMenu.SetBool(strStateString, true);
                    startWaitTimer = startWaitTime;
                    break;
                case FDState.InGame:
                    if (!isHiding && postHidingTimer < 0)
                    {
                        ShovelAttackStart();
                    }
                    break;
                case FDState.Lost:
                    if (startWaitTimer > 0)
                    {
                        break;
                    }
                    animMainMenu.SetBool(strStateString, false);
                    gameState = FDState.MainMenu;
                    txtEndScreen.text = "";
                    startWaitTimer = startWaitTime;
                    DestroyEnemy();
                    break;
            }
        }

        void HideButtonPress()
        {
            if (playerState == FDPlayerState.Idle || playerState == FDPlayerState.Hiding)
            {
                if (isHiding)
                {
                    UnHide();
                }
                else
                {
                    if (playerStamina > 0)
                    {
                        Hide();
                    }
                }
            }
        }

        public void DestroyEnemy()
        {
            Destroy(currentEnemy.gameObject);
            currentEnemy = null;
            isEnemyInFront = false;

            animButtonTooltips.SetBool("Active", false);

            if (playerState != FDPlayerState.Latched && playerState != FDPlayerState.Hiding && playerState != FDPlayerState.Dead)
            {
                playerState = FDPlayerState.Walking;
            }
            if (IsOwner)
            {
                DestroyEnemyClientRpc();
            }
        }

        [ServerRpc]
        public void DestroyEnemyServerRpc()
        {
            if(currentEnemy != null)
            {
                DestroyEnemy();
            }
        }

        [ClientRpc]
        public void DestroyEnemyClientRpc()
        {
            if (!IsOwner && currentEnemy)
            {
                DestroyEnemy();
            }
        }

        public override void GameUpdate()
        {
            base.GameUpdate();

            if (gameState != FDState.InGame)
            {
                return;
            }


            if (playerState == FDPlayerState.Latched)
            {
                if (playerHealth > 1)
                {
                    if (damageOverTimeTimer > 0)
                    {
                        damageOverTimeTimer -= Time.deltaTime;
                    }
                    else
                    {
                        SetPlayerHealth(playerHealth - 1);
                        damageOverTimeTimer = damageOverTimeTime;
                    }
                    return;
                }
                else
                {
                    UnLatch();
                }
            }
            else if (playerState == FDPlayerState.Walking)
            {
                if (startWaitTimer <= 0)
                {
                    if (IsOwner)
                    {
                        currentFrameDistance = gameSpeed * Time.deltaTime;
                        currentScore.Value += currentFrameDistance;
                        hallwayStateDistanceTraveled.Value += currentFrameDistance;
                    }
                }

                switch (hallwayState)
                {
                    case FDHallwayState.Enter:
                        distanceMod = (int)hallwayStateDistanceTraveled.Value % (hallwayEnterFrameCount + 1);
                        if (distanceMod == hallwayEnterFrameCount)
                        {
                            ChangeHallwayState(FDHallwayState.Inside);
                        }
                        break;
                    case FDHallwayState.Inside:
                        distanceMod = (int)hallwayStateDistanceTraveled.Value % (hallwayFrameCount);
                        if (distanceMod == 0 && currentScore.Value > currentLevelDistanceGoal)
                        {
                            ChangeHallwayState(FDHallwayState.Exit);
                        }
                        break;
                    case FDHallwayState.Exit:
                        distanceMod = (int)hallwayStateDistanceTraveled.Value % (hallwayExitFrameCount + 1);
                        if (distanceMod == hallwayExitFrameCount)
                        {
                            animHallway.SetInteger(strStateString, hallwayExitFrameCount);
                            playerState = FDPlayerState.Done;
                            shovel.gameObject.SetActive(false);
                            animPlayerHealth.gameObject.SetActive(false);
                            animPlayerStamina.gameObject.SetActive(false);
                            ShowLevelComplete(0);
                            return;
                        }
                        break;
                }

                if (oldDistanceMod != distanceMod)
                {
                    animHallway.SetInteger(strStateString, distanceMod);
                }


                shovelWalkYOffset = -(2 - ((int)currentScore.Value % 3));

                if (currentEnemy != null)
                {
                    if (currentEnemy.distance < currentScore.Value)
                    {
                        if (IsOwner)
                        {
                            currentScore.Value = currentEnemy.distance;
                            EnemyInFront();
                        }
                    }
                }
            }
            else if (playerState == FDPlayerState.Done)
            {
                levelTransitionTimer -= Time.deltaTime;
                if (levelTransitionPhase == 0)
                {
                    if (levelTransitionTimer < levelTransitionTime / 2)
                    {
                        ShowLevelComplete(1);
                    }
                }
                else if (levelTransitionPhase == 1)
                {
                    if (levelTransitionTimer < 0)
                    {
                        NextLevel();
                    }
                }
            }
            else
            {
                shovelWalkYOffset = 0;
            }



            if (playerState == FDPlayerState.Hiding)
            {
                playerStamina -= staminaLossPerSec * Time.deltaTime;
                if (playerStamina > -1.5)
                {
                    if (playerStamina >= -1)
                    {
                        animPlayerStamina.SetInteger(strStateString, Mathf.CeilToInt(playerStamina));
                    }
                }
                else
                {
                    UnHide();
                }
            }
            else
            {
                if (playerStamina < basePlayerStamina)
                {
                    playerStamina += staminaLossPerSec / 2 * Time.deltaTime;
                    animPlayerStamina.SetInteger(strStateString, Mathf.CeilToInt(playerStamina));
                }
                else
                {
                    playerStamina = basePlayerStamina;
                }
            }

            txtScore.text = ((int)currentScore.Value).ToString();

            if (animHallway.GetInteger(strStateString) == FrameAddSub(hallwayMonsterSpawnFrame, -4) && hallwayState == FDHallwayState.Inside)
            {
                if (IsOwner)
                {
                    if (!hasEnemyBeenRolled)
                    {
                        RollForMonsterSpawn((int)currentScore.Value + 5);
                        hasEnemyBeenRolled = true;
                    }
                }
            }
            else
            {
                hasEnemyBeenRolled = false;
            }


            if (isHoldingButton)
            {
                Hide();
            }
            else
            {
                if (isHiding)
                {
                    UnHide();
                }
                else
                {
                    postHidingTimer -= Time.deltaTime;
                }
            }

            if (hideButton)
            {
                HideButtonPress();
                hideButton = false;
            }

            animShovel.SetInteger(strBobString, shovelWalkYOffset);
            oldDistanceMod = distanceMod;
        }

        void StartGame()
        {
            levelIndex = 0;
            if (IsOwner)
            {
                currentScore.Value = 0;
            }
            lives = 1;
            playerState = FDPlayerState.Walking;
            gameState = FDState.InGame;
            animHallway.SetInteger(strStateString, 0);
            isHiding = false;

            animPlayerHealth.gameObject.SetActive(true);
            animPlayerStamina.gameObject.SetActive(true);

            animGame.SetBool(strDeadString, false);
            animGame.SetBool("BrackenDead", false);

            animShovel.ResetTrigger(strHitString);
            animShovel.ResetTrigger(strGotHitString);
            animShovel.SetBool(strDeadString, false);
            animShovel.SetBool(strIsWalkingString, true);
            txtEndScreen.text = "";

            enemyTimesEncountered.Clear();

            SetupLevel(0);
        }

        void SetupLevel(int levelIndex)
        {
            if (levelIndex < levels.Count)
            {
                SetupEnemyWeight(levelIndex);
            }
            else
            {
                FD_EnemyWeight[] enemyList = {
                new FD_EnemyWeight(prefabThumper.GetComponent < FD_Enemy >(), 90, FDEnemyType.Thumper),
                new FD_EnemyWeight(prefabBunkerSpider.GetComponent < FD_Enemy >(), 40, FDEnemyType.BunkerSpider),
                new FD_EnemyWeight(prefabLootBug.GetComponent < FD_Enemy >(), 60, FDEnemyType.LootBug),
                new FD_EnemyWeight(prefabSnareFlea.GetComponent < FD_Enemy >(), 40, FDEnemyType.SnareFlea),
                new FD_EnemyWeight(prefabBracken.GetComponent < FD_Enemy >(), 20, FDEnemyType.Bracken),
                new FD_EnemyWeight(prefabNutcracker.GetComponent < FD_Enemy >(), 20, FDEnemyType.Nutcracker)
            };
                levels.Add(new FD_Level(levelIndex, gameSpeed + gameSpeedIncrement, 100, .75f, enemyList.ToList()));
            }

            gameSpeed = levels[levelIndex].speed;
            currentMonsterSpawnChance = levels[levelIndex].monsterSpawnChance;
            previousDistanceTraveled = currentScore.Value;
            currentLevelDistanceGoal = levels[levelIndex].distanceLength + previousDistanceTraveled;

            animShovel.speed = 1 * (gameSpeed / startingGameSpeed);
            animGame.speed = 1 * (gameSpeed / startingGameSpeed);
            SetPlayerHealth(basePlayerHealth);
            playerStamina = basePlayerStamina;
            animPlayerStamina.SetInteger(strStateString, Mathf.CeilToInt(playerStamina));

            ChangeHallwayState(FDHallwayState.Enter);

            playerState = FDPlayerState.Walking;
            txtLevelEndScreen.text = "";


            if (levelIndex < 4)
            {
                PlaySound(acSong1);
            }
            else
            {
                PlaySound(acSong2);
            }

            Debug.Log("PintoBoy FD: Level " + levelIndex + " loaded");
        }

        void NextLevel()
        {
            shovel.gameObject.SetActive(true);
            animPlayerHealth.gameObject.SetActive(true);
            animPlayerStamina.gameObject.SetActive(true);
            levelIndex++;
            levelTransitionPhase++;
            SetupLevel(levelIndex);
        }

        void ShowLevelComplete(int phase)
        {
            StopSounds();
            if (phase == 0)
            {
                txtLevelEndScreen.text = "Facility " + (levelIndex + 1) + "\nComplete";
                levelTransitionTimer = levelTransitionTime;
                levelTransitionPhase = 0;
                PlaySound(acFacilityCleared);
            }
            else
            {
                txtLevelEndScreen.text = "Entering\nFacility " + (levelIndex + 2);
                levelTransitionPhase = phase;
            }
        }

        void SetupEnemyWeight(int levelIndex)
        {
            for (int i = 0; i < levels[levelIndex].enemyList.Count; i++)
            {
                SetupAddEnemy(levels[levelIndex].enemyList[i].enemy, levels[levelIndex].enemyList[i].weight, levels[levelIndex].enemyList[i].type);
            }

            SetupSetTotalWeight();
        }

        int FrameAddSub(int frame, int addsub)
        {
            int value = frame + addsub;
            if (value > hallwayFrameCount)
            {
                return value = value - hallwayFrameCount;
            }
            else if (value < 0)
            {
                return value = hallwayFrameCount + value;
            }
            return value;
        }

        void RollForMonsterSpawn(int distance)
        {
            float rand = UnityEngine.Random.value;
            if (rand < currentMonsterSpawnChance)
            {
                SpawnRandomEnemy(distance);
                Debug.Log("PintoBoy FD: Monster roll success");
            }
            else
            {
                Debug.Log("PintoBoy FD: Monster roll failed");
            }
        }

        public void EnemyInFront()
        {
            playerState = FDPlayerState.Idle;
            isEnemyInFront = true;
            if (!enemyTimesEncountered.ContainsKey(currentEnemy.enemyName))
            {
                enemyTimesEncountered.Add(currentEnemy.enemyName, 1);
                animButtonTooltips.SetBool("Active", true);
                string name = currentEnemy.enemyName;
                switch (name)
                {
                    case "Spider":
                        animButtonTooltips.SetTrigger("Attack");
                        break;
                    case "Snare Flea":
                        animButtonTooltips.SetTrigger("Attack");
                        break;
                    case "Bracken":
                        animButtonTooltips.SetTrigger("Hide");
                        break;
                    case "Thumper":
                        animButtonTooltips.SetTrigger("Attack");
                        break;
                    case "Nutcracker":
                        animButtonTooltips.SetTrigger("Wait");
                        break;
                    case "Loot Bug":
                        animButtonTooltips.SetTrigger("Wait");
                        break;
                    default:
                        break;
                }
            }
            else
            {
                enemyTimesEncountered[currentEnemy.enemyName] += 1;
            }
            currentEnemy.IsInFront();
        }

        void ShovelAttackStart()
        {
            animShovel.SetTrigger(strHitString);
        }

        void ShovelAttackConnected()
        {
            if (currentEnemy != null && isEnemyInFront)
            {
                //Debug.Log("PintoBoy FD: attack connected:"+currentEnemy.name);
                bool isDead = currentEnemy.TakeDamage();
                float rand = UnityEngine.Random.value;
                if (rand > 0)
                {
                    PlaySound(acPlayerAttack);
                }
                else
                {
                    //PlaySound(acPlayerAttack2);
                }

                if (isDead)
                {
                    DestroyEnemy();
                }
            }
        }

        void ChangeHallwayState(FDHallwayState state)
        {
            hallwayState = state;
            if (IsOwner)
            {
                hallwayStateDistanceTraveled.Value = 0;
            }
            animHallway.SetInteger(strEnterExitString, (int)hallwayState);
        }

        public void Latch()
        {
            if (playerHealth == 1)
            {
                KillPlayer(false);
                return;
            }

            if (lives > 1)
            {

            }
            else
            {
                playerState = FDPlayerState.Latched;
                shovel.TurnOff();
                damageOverTimeTimer = damageOverTimeTime;
            }
        }

        void UnLatch()
        {
            Destroy(currentEnemy.gameObject);
            currentEnemy = null;

            playerState = FDPlayerState.Walking;
            shovel.TurnOn();
        }

        void SetPlayerHealth(int newValue)
        {
            playerHealth = newValue;
            if (playerHealth > 0)
            {
                animPlayerHealth.SetInteger(strStateString, playerHealth);
            }
        }

        private void SetupSetTotalWeight()
        {
            totalWeight = 0;

            for (int i = 0; i < currentEnemyList.Count; i++)
            {
                totalWeight += currentEnemyList[i].weight;
            }
        }

        void SetupAddEnemy(FD_Enemy enemy, int weight, FDEnemyType type)
        {
            currentEnemyList.Add(new FD_EnemyWeight(enemy, weight, type));
        }

        void SpawnRandomEnemy(int distance)
        {
            int rand = UnityEngine.Random.Range(0, totalWeight);
            int cumulativeWeight = 0;

            for (int i = 0; i < currentEnemyList.Count; i++)
            {
                cumulativeWeight += currentEnemyList[i].weight;
                if (rand <= cumulativeWeight)
                {
                    SpawnEnemyServerRpc(currentEnemyList[i].type.ToString(), distance);
                    //SpawnEnemy(prefabThumper.GetComponent<FD_Enemy>(), distance);
                    return;
                }
            }
        }

        FD_Enemy SpawnEnemy(FD_Enemy prefab, int distance)
        {
            if(prefab == null)
            {
                Debug.Log("bruh, this is null");
            }
            try
            {

                //Debug.Log("PintoBoy FD: Spawning " + prefab.name);
                FD_Enemy enemy = Instantiate(prefab, Vector3.zero, Quaternion.identity, trEnemySpawn).GetComponent<FD_Enemy>();
                enemy.distance = distance;
                enemy.game = this;
                enemy.dead.AddListener(DestroyEnemy);

                enemy.animator.SetFloat("Speed", gameSpeed / startingGameSpeed);
                currentEnemy = enemy;
                return enemy;
            }
            catch (Exception ex)
            {
                Debug.Log("Error when trying to spawnEnemy in Facility Dash"+ex);
                return null;
            }
        }

        [ServerRpc]
        void SpawnEnemyServerRpc(string enemy, int distance)
        {
            //Debug.Log("PintoBoy FD: attempting to spawn "+enemy+" serverside");
            //FDEnemyType enemyType = Enum.Parse<FDEnemyType>(enemy);
            //Debug.Log("PintoBoy FD: enemy enum parsed: " + enemyType.ToString());
            //FD_Enemy newEnemy = SpawnEnemy(EnumToEnemy(enemyType), distance);
            //Debug.Log("PintoBoy FD: enemy spawned locally");
            SpawnEnemyClientRpc(enemy, distance);
        }

        [ClientRpc]
        void SpawnEnemyClientRpc(string enemy, int distance)
        {
            Debug.Log("PintoBoy FD: CLient spawning "+enemy);
            FDEnemyType enemyType = Enum.Parse<FDEnemyType>(enemy);
            Debug.Log("PintoBoy FD: enemy enum parsed: " + enemyType.ToString());
            FD_Enemy newEnemy = SpawnEnemy(EnumToEnemy(enemyType), distance);
            Debug.Log("PintoBoy FD: enemy spawned locally through client");
        }

        FD_Enemy EnumToEnemy(FDEnemyType enemyType)
        {
            FD_Enemy newPrefab;
            switch (enemyType)
            {
                case FDEnemyType.Bracken:
                    newPrefab = Pinto_ModBase.fdBrackenPrefab.GetComponent<FD_Enemy>();
                    break;
                case FDEnemyType.BunkerSpider:
                    newPrefab = Pinto_ModBase.fdBunkerSpiderPrefab.GetComponent<FD_Enemy>();
                    break;
                case FDEnemyType.LootBug:
                    newPrefab = Pinto_ModBase.fdLootBugPrefab.GetComponent<FD_Enemy>();
                    break;
                case FDEnemyType.Nutcracker:
                    newPrefab = Pinto_ModBase.fdNutcrackerPrefab.GetComponent<FD_Enemy>();
                    break;
                case FDEnemyType.SnareFlea:
                    newPrefab = Pinto_ModBase.fdSnareFleaPrefab.GetComponent<FD_Enemy>();
                    break;
                case FDEnemyType.Thumper:
                    newPrefab = Pinto_ModBase.fdThumperPrefab.GetComponent<FD_Enemy>();
                    break;
                default:
                    Debug.Log("PintoBoy FD: EnumToEnemy failed. Results:"+enemyType.ToString()+". Spawning Thumper");
                    newPrefab = Pinto_ModBase.fdThumperPrefab.GetComponent<FD_Enemy>();
                    break;
            }
            return newPrefab;
        }


        public int GetDistanceTraveled()
        {
            return (int)currentScore.Value;
        }

        public void PlayerIsAttacked(int damage, bool isAttackBlockedByHiding)
        {
            if (!isHiding || !isAttackBlockedByHiding)
            {
                animButtonTooltips.SetBool("Active", false);
                animShovel.SetTrigger(strGotHitString);
                SetPlayerHealth(playerHealth - damage);
                if (playerHealth <= 0)
                {
                    if (damage == 5)
                    {
                        KillPlayer(true);
                    }
                    else
                    {
                        KillPlayer(false);
                    }
                }
            }
        }

        void KillPlayer(bool isBracken)
        {
            if (gameState == FDState.Lost) { return; }
            playerState = FDPlayerState.Dead;

            animButtonTooltips.SetBool("Active", false);
            animPlayerHealth.gameObject.SetActive(false);
            animPlayerStamina.gameObject.SetActive(false);
            StopSounds();
            if (isBracken)
            {
                animGame.SetBool("BrackenDead", true);
                PlaySound(acSnapNeck);
            }
            else
            {
                animGame.SetBool(strDeadString, true);
            }
            animShovel.SetBool(strDeadString, true);
            startWaitTimer = startWaitTime / 2;
        }

        public void Dead()
        {
            ShowEndScreen();
            gameState = FDState.Lost;
        }

        void ShowEndScreen()
        {
            Debug.Log("PintoBoy FD: Showing end screen");
            if (currentScore.Value > highscore)
            {
                txtEndScreen.text = $"New Best!\n" +
                                     $"{Mathf.Round(currentScore.Value)}\n" +
                                     $"Last Best\n" +
                                     $"{Mathf.Round(highscore)}";
                if (IsOwner)
                {
                    highScore.Value = (int)currentScore.Value;
                }
                PlaySound(acNewHighscore);
            }
            else
            {
                txtEndScreen.text = $"Score\n" +
                                     $"{Mathf.Round(currentScore.Value)}\n" +
                                     $"Best\n" +
                                     $"{Mathf.Round(highscore)}";
                PlaySound(acNoHighscore);
            }
        }

        public override void InitializeObjects(Transform gameRoot)
        {
            base.InitializeObjects(gameRoot);

            Debug.Log("PintoBoy FD: intializing FacilityDash");
            if (gameRoot != null)
            {
                Debug.Log("PintoBoy FD: root name: " + gameRoot.name);
                Debug.Log("PintoBoy FD: child name: " + gameRoot.GetChild(0).name);
                Debug.Log("PintoBoy FD: parent name: " + gameRoot.parent.name);
            }
            else
            {
                Debug.Log("PintoBoy FD: gameRoot null");
            }

            trMainMenu = gameRoot.transform.Find("Main Menu");
            trMainMenu.gameObject.SetActive(true);
            animMainMenu = trMainMenu.transform.Find("Main Menu Sprite").GetComponent<Animator>();
            trInGame = gameRoot.transform.Find("Game");

            animGame = trInGame.GetComponent<Animator>();

            animHallway = trInGame.transform.Find("Hallway").GetComponent<Animator>();

            Debug.Log("PintoBoy FD: initializing shovel");
            trShovel = trInGame.transform.Find("Shovel");
            shovel = trShovel.gameObject.AddComponent<FD_Shovel>();
            Debug.Log("PintoBoy FD: added shovel script");
            shovel.hit.AddListener(ShovelAttackConnected);
            shovel.dead.AddListener(Dead);
            trShovelSpawnpoint = trInGame.transform.Find("Shovel Spawnpoint");
            animShovel = trShovel.GetComponent<Animator>();
            Debug.Log("PintoBoy FD: shovel done");

            trEnemySpawn = trInGame.transform.Find("Current Enemy");


            trUI = trInGame.transform.Find("UI");
            animPlayerHealth = trUI.Find("PlayerHealth").GetComponent<Animator>();
            animPlayerStamina = trUI.Find("PlayerStamina").GetComponent<Animator>();
            animButtonTooltips = trUI.Find("Tooltips").GetComponent<Animator>();
            txtScore = trUI.transform.Find("Score").GetComponent<TMP_Text>();
            txtEndScreen = trUI.transform.Find("Death Screen/Text").GetComponent<TMP_Text>();
            txtLevelEndScreen = trUI.transform.Find("Level End Screen/Text").GetComponent<TMP_Text>();

            animFade = gameRoot.transform.Find("Fade").GetComponent<Animator>();
            animFade.gameObject.SetActive(true);

            txtScore.text = "";
            txtEndScreen.transform.parent.gameObject.SetActive(true);
            txtEndScreen.text = "";
            txtLevelEndScreen.text = "";
        }
    }

}