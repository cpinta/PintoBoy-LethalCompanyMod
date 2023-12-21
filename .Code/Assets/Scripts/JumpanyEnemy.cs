using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public enum PintoEnemyType
{
    Spider = 0,
    Slime = 1,
    Lootbug = 2
}

public class JumpanyEnemy : NetworkBehaviour
{
    public PintoEnemyType enemyType;
    public float speed = 5f;
    float startX;
    float distance;
    public PintoBoy pintoBoy;
    public UnityEvent<JumpanyEnemy> onDeath = new UnityEvent<JumpanyEnemy>();

    public Animator animator;
    public bool paused = false;

    public bool killedPlayer = false;

    AudioClip[] movementSounds;
    float currentMovementSoundTimer = 0f;
    int currentMovementSoundIndex = 0;

    public NetworkVariable<int> parentNetworkID = new NetworkVariable<int>();

    // Start is called before the first frame update
    void Awake()
    {
        distance = 10;
        animator = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (pintoBoy.isTurnedOff)
        {
            return;
        }

        if (killedPlayer)
        {
            if (enemyType == PintoEnemyType.Spider)
            {
                animator.enabled = false;
            }
            return;
        }

        float difference = speed * Time.deltaTime;

        this.transform.position += Vector3.left * difference;

        distance -= difference;

        if (distance < 0)
        {
            onDeath.Invoke(this);
        }


        currentMovementSoundTimer -= Time.deltaTime;
        if (currentMovementSoundTimer < 0)
        {
            pintoBoy.PlaySound(movementSounds[currentMovementSoundIndex]);
            currentMovementSoundIndex++;
            if (currentMovementSoundIndex >= movementSounds.Length)
            {
                currentMovementSoundIndex = 0;
            }
            currentMovementSoundTimer = movementSounds[currentMovementSoundIndex].length;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if(pintoBoy != null)
            {
                pintoBoy.PlayerGotHit();
            }
        }
    }

    public void SetMovementSounds(AudioClip[] audioClips)
    {
        movementSounds = audioClips;
        currentMovementSoundTimer = movementSounds[0].length;
    }
}
