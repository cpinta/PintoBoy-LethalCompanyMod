using LethalLib.Modules;
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

namespace PintoMod.Assets.Scripts.LethalJumpany
{
    public enum LJState
    {
        MainMenu,
        InGame,
        Paused,
        Lost
    }


    public class LethalJumpany : PintoBoyGame
    {

        LJState gameState = LJState.MainMenu;

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


        public float jumpHeight = 9.75f;
        public float fastFallSpeed = 15f;
        public float rayCastDistance = 0.5f;
        public float rayCastOffset = -0.05f;

        public bool jump = false;
        bool isGrounded = false;

        bool doOnce = false;

        Transform paused;

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

        List<LJEnemy> enemies = new List<LJEnemy>();

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

        // Start is called before the first frame update
        void Awake()
        {

            acPlayerStep1 = Pinto_ModBase.GetAudioClip(Pinto_ModBase.ljAudioPath + "21_walk1 (player step 1)");
            acPlayerStep2 = Pinto_ModBase.GetAudioClip(Pinto_ModBase.ljAudioPath + "22_walk2 (player step 2)");
            acPlayerJump = Pinto_ModBase.GetAudioClip(Pinto_ModBase.ljAudioPath + "23_ladder (player land)");

            acSnapNeck = Pinto_ModBase.GetAudioClip(Pinto_ModBase.ljAudioPath + "12_exchange (snap neck)");
            acSnapNeckLand = Pinto_ModBase.GetAudioClip(Pinto_ModBase.ljAudioPath + "67_knock (fall after neck snap)");

            acSpiderStep1 = Pinto_ModBase.GetAudioClip(Pinto_ModBase.ljAudioPath + "52_step1 (spider movement)");
            acSpiderStep2 = Pinto_ModBase.GetAudioClip(Pinto_ModBase.ljAudioPath + "53_step2 (spider movement)");
            acSpiderStep3 = Pinto_ModBase.GetAudioClip(Pinto_ModBase.ljAudioPath + "54_step3 (spider movement)");
            acSpiderStep4 = Pinto_ModBase.GetAudioClip(Pinto_ModBase.ljAudioPath + "55_step4 (spider movement)");

            acLootbugStep = Pinto_ModBase.GetAudioClip(Pinto_ModBase.ljAudioPath + "47_grass (lootbug movement)");

            acSlimeStep = Pinto_ModBase.GetAudioClip(Pinto_ModBase.ljAudioPath + "30_triangle (slime movement)");

            acConfirm = Pinto_ModBase.GetAudioClip(Pinto_ModBase.ljAudioPath + "59_confirm (start game)");

            acNewHighscore = Pinto_ModBase.GetAudioClip(Pinto_ModBase.ljAudioPath + "24_levelclear (new highscore)");
            acNoHighscore = Pinto_ModBase.GetAudioClip(Pinto_ModBase.ljAudioPath + "64_lose2 (no highscore)");

            acBackgroundSong = Pinto_ModBase.GetAudioClip(Pinto_ModBase.ljAudioPath + "danger_streets");
            backgroundMusicTime = acBackgroundSong.length;

            spiderPrefab = Pinto_ModBase.ljSpiderPrefab;
            lootbugPrefab = Pinto_ModBase.ljLootbugPrefab;
            slimePrefab = Pinto_ModBase.ljSlimePrefab;

            mainmenuWaitTimer = mainmenuWaitTime;
        }

        public override void GameUpdate()
        {
            base.GameUpdate();
            switch (gameState)
            {
                case LJState.MainMenu:
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
                        pintoBoy.SetFade(FadeState.FadeOut);
                        doOnce = true;
                    }
                    if (mainmenuTimer <= 1 && mainmenuTimer > 0 && doOnce)
                    {
                        mainmenuTimer = 0f;
                        StartGame();
                        pintoBoy.SetFade(FadeState.FadeIn);
                        doOnce = false;
                    }
                    break;
                case LJState.InGame:
                    LJinGameUpdate();
                    break;
                case LJState.Paused:
                    break;
                case LJState.Lost:
                    break;
            }
        }

        void LJinGameUpdate()
        {
            if (endScreenShown)
            {
                return;
            }
            if (!deadAnimStarted)
            {
                RaycastHit2D hit = Physics2D.Raycast(player.transform.position - rayCastOffset * Vector3.down, Vector2.down, rayCastDistance);

                if (hit.collider == groundCol)
                {
                    isGrounded = true;
                    Debug.DrawRay(player.transform.position - rayCastOffset * Vector3.down, Vector2.down * rayCastDistance, Color.green);
                    playerAnim.SetBool(InAirString, false);
                    deadHitGround = true;
                }
                else
                {
                    isGrounded = false;
                    Debug.DrawRay(player.transform.position - rayCastOffset * Vector3.down, Vector2.down * rayCastDistance, Color.red);
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


            if (pintoBoy.IsOwner)
            {
                Debug.Log("currentScore setting");
                currentScore.Value += scoreIncreaseRate * Time.deltaTime;
                Debug.Log("currentScore set");
            }

            if (currentScore.Value > speedAdditionMultiplier * increaseAdditionRate && speedAdditionMultiplier > timesIncreased)
            {
                timesIncreased++;
                speedAdditionMultiplier++;
            }

            if (pintoBoy.IsOwner)
            {
                if (currentScore.Value > lastTimeSpawned + Random.Range(spawnEnemyEveryMin, spawnEnemyEveryMax))
                {
                    SpawnRandomEnemyServerRpc();
                    lastTimeSpawned = currentScore.Value;
                }
            }

            scoreText.text = Mathf.Round(currentScore.Value).ToString();

            if (player.transform.localPosition.y < playerStart.y - 0.5f || player.transform.localPosition.y > playerStart.y + 100)
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
                bracken.transform.localPosition = new Vector3(playerSpawnpoint.localPosition.x - playerPositions[lives.Value] * 3, brackenStart.y, 0);
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

        LJEnemy EnumToJumpanyEnemy(PintoEnemyType enemyType)
        {
            LJEnemy newPrefab = new LJEnemy();
            switch (enemyType)
            {
                case PintoEnemyType.Spider:
                    newPrefab = Pinto_ModBase.ljSpiderPrefab.GetComponent<LJEnemy>();
                    break;
                case PintoEnemyType.Slime:
                    newPrefab = Pinto_ModBase.ljSlimePrefab.GetComponent<LJEnemy>();
                    break;
                case PintoEnemyType.Lootbug:
                    newPrefab = Pinto_ModBase.ljLootbugPrefab.GetComponent<LJEnemy>();
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
            LJEnemy newEnemy = SpawnEnemy(EnumToJumpanyEnemy(enemyType), EnumToEnemySpawnPoint(enemyType), speed, enemyType, EnumToEnemySounds(enemyType));
            SpawnEnemyClientRpc(speed, enemy);
        }

        [ClientRpc]
        void SpawnEnemyClientRpc(float speed, string enemy)
        {
            PintoEnemyType enemyType = Enum.Parse<PintoEnemyType>(enemy);

            LJEnemy newEnemy = SpawnEnemy(EnumToJumpanyEnemy(enemyType), EnumToEnemySpawnPoint(enemyType), speed, enemyType, EnumToEnemySounds(enemyType));
        }

        LJEnemy SpawnEnemy(LJEnemy prefab, Transform position, float speed, PintoEnemyType enemy, AudioClip[] audioClips)
        {
            LJEnemy enemyObj = Instantiate(prefab, position.position, Quaternion.identity, position);
            enemyObj.speed = speed + increaseSpeedAddition * speedAdditionMultiplier;
            enemyObj.game = this;
            enemyObj.onDeath.AddListener(OnEnemyDeath);
            enemyObj.enemyType = enemy;
            enemyObj.SetMovementSounds(audioClips);
            enemies.Add(enemyObj);
            return enemyObj;
        }

        void OnEnemyDeath(LJEnemy enemy)
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

        public override void ButtonPress()
        {
            base.ButtonPress();

            Debug.Log("Pressed Button LJ");

            switch (gameState)
            {
                case LJState.MainMenu:
                    if (mainmenuWaitTimer <= 0 && mainmenuTimer <= 0)
                    {
                        StartFromTitleScreen();
                    }
                    else
                    {
                        return;
                    }
                    break;
                case LJState.InGame:
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
                case LJState.Paused:
                    break;
                case LJState.Lost:
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
            SwitchState(LJState.MainMenu);
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

            SwitchState(LJState.InGame);

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

            if (pintoBoy.IsOwner)
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
                foreach (LJEnemy enemy in enemies)
                {
                    Destroy(enemy.gameObject);
                }
                enemies.Clear();
            }
        }

        void ClearEnemiesExcept(LJEnemy enemy)
        {
            if (enemies.Count > 0)
            {
                foreach (LJEnemy enemys in enemies)
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
        void SwitchStateServerRpc(LJState newState)
        {
            SwitchState(newState);
            SwitchStateClientRpc(newState);
        }

        [ClientRpc]
        void SwitchStateClientRpc(LJState newState)
        {
            SwitchState(newState);
        }

        void SwitchState(LJState newState)
        {

            switch (newState)
            {
                case LJState.MainMenu:
                    mainMenu.gameObject.SetActive(true);
                    inGame.gameObject.SetActive(false);
                    paused.gameObject.SetActive(false);
                    doOnce = false;
                    break;
                case LJState.InGame:
                    mainMenu.gameObject.SetActive(false);
                    inGame.gameObject.SetActive(true);
                    paused.gameObject.SetActive(false);
                    break;
                case LJState.Paused:
                    mainMenu.gameObject.SetActive(false);
                    inGame.gameObject.SetActive(false);
                    paused.gameObject.SetActive(true);
                    break;
                case LJState.Lost:
                    mainMenu.gameObject.SetActive(false);
                    inGame.gameObject.SetActive(false);
                    paused.gameObject.SetActive(false);
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

        public override void Pause()
        {
            base.Pause();
            DisableAllAnimators();
        }

        public override void UnPause()
        {
            base.UnPause();
            EnableAllAnimators();
            if (gameState == LJState.InGame && !dead)
            {
                pintoBoy.PlaySound(acBackgroundSong);
            }
        }

        public override void TurnedOn()
        {
            EnableAllAnimators();

            ClearEnemies();
            ShowTitleScreen();
        }

        public override void TurnedOff()
        {
            DisableAllAnimators();
        }

        void EnableAllAnimators()
        {
            groundAnim.enabled = true;
            brackenAnim.enabled = true;
            mainMenuAnim.enabled = true;
            playerAnim.enabled = true;
            pintoBoy.EnableFade(true);

            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].animator.enabled = true;
                enemies[i].paused = true;
            }
        }

        void DisableAllAnimators()
        {
            if (groundAnim != null)
            {
                Debug.Log("groundAnim.enabled = false;");
                groundAnim.enabled = false;
            }
            else
            {
                Debug.LogWarning("groundAnim is null.");
            }

            if (brackenAnim != null)
            {
                Debug.Log("brackenAnim.enabled = false;");
                brackenAnim.enabled = false;
            }
            else
            {
                Debug.LogWarning("brackenAnim is null.");
            }

            if (mainMenuAnim != null)
            {
                Debug.Log("mainMenuAnim.enabled = false;");
                mainMenuAnim.enabled = false;
            }
            else
            {
                Debug.LogWarning("mainMenuAnim is null.");
            }

            if (playerAnim != null)
            {
                Debug.Log("playerAnim.enabled = false;");
                playerAnim.enabled = false;
            }
            else
            {
                Debug.LogWarning("playerAnim is null.");
            }


            pintoBoy.EnableFade(false);

            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].animator.enabled = false;
                enemies[i].paused = false;
            }
        }

        public void PlayerGotHit()
        {
            if (!pintoBoy.IsOwner)
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

            for (int i = 0; i < enemies.Count; i++)
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
            pintoBoy.StopSounds();
        }

        void ShowEndScreen()
        {
            if (currentScore.Value > highScore.Value)
            {
                endScreenText.text = $"New Best!\n" +
                                     $"{Mathf.Round(currentScore.Value)}\n" +
                                     $"Last Best:\n" +
                                     $"{Mathf.Round(highScore.Value)}";
                if (pintoBoy.IsOwner)
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
            pintoBoy.PlaySound(clip);
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

        public override void InitializeObjects(Transform gameRoot)
        {
            base.InitializeObjects(gameRoot);

            Debug.Log("intiializing LethalJumpany");

            mainMenu = gameRoot.transform.Find("Main Menu");
            mainMenuAnim = mainMenu.transform.Find("Main Menu Sprite").GetComponent<Animator>();

            inGame = gameRoot.transform.Find("Game");

            player = inGame.transform.Find("PintoEmployee").gameObject;
            playerStart = player.transform.localPosition;
            playerRb = player.GetComponent<Rigidbody2D>();
            playerAnim = player.GetComponent<Animator>();
            playerCol = player.GetComponent<Collider2D>();
            playerSprite = player.GetComponent<SpriteRenderer>();

            bracken = inGame.transform.Find("Bracken").gameObject;
            brackenAnim = bracken.GetComponent<Animator>();
            brackenStart = bracken.transform.localPosition;

            groundCol = inGame.transform.Find("RailingGround").GetComponent<Collider2D>();
            groundAnim = groundCol.gameObject.GetComponent<Animator>();

            scoreText = inGame.transform.Find("UI/Score").GetComponent<TMP_Text>();
            endScreenText = inGame.transform.Find("UI/Death Screen/Text").GetComponent<TMP_Text>();

            paused = gameRoot.transform.Find("Paused");

            playerRb.bodyType = RigidbodyType2D.Kinematic;

            topSpawnpoint = inGame.transform.Find("Top Spawnpoint");
            midSpawnpoint = inGame.transform.Find("Mid Spawnpoint");
            bottomSpawnpoint = inGame.transform.Find("Bottom Spawnpoint");
            playerSpawnpoint = inGame.transform.Find("Player Spawnpoint");

            endScreenText.text = "";
        }
    }
}
