﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;


namespace PintoMod.Assets.Scripts.FacilityDash
{
    public abstract class FD_Enemy : MonoBehaviour
    {
        protected Animator animator;
        protected SpriteRenderer spriteRenderer;
        public FacilityDash game;

        public UnityEvent dead;
        public UnityEvent<int> playerDamage;

        protected AudioClip acEntrance;

        protected string strAttackModeString = "AttackMode";
        protected string strAttackString = "Attack";
        protected string strHurtString = "Hurt";
        protected string strLeaveString = "Leave";
        protected string strSpeedString = "Speed";

        public string enemyName = "";
        protected string EnemyName
        {
            get { return enemyName; }
            set { enemyName = value; }
        }

        private int health = 1;
        protected int Health
        {
            get { return health; }
            set { health = value; }
        }

        private float attackSpeed = 5f;     //when the enemy attacks (seconds)
        protected float AttackSpeeed
        {
            get { return attackSpeed; }
            set { attackSpeed = value; }
        }

        private int attackDamage = 1;
        protected int AttackDamage
        {
            get { return attackDamage; }
            set { attackDamage = value; }
        }

        float attackTimer = 0;

        protected bool leaving = false;
        float leaveLengthTime = 1.25f;
        protected float leaveTimer = 0;

        public int distance;
        protected int distanceDifference;
        protected int oldDistanceDifference;
        protected bool gameOver;

        public bool usesDefaultHurtAnim = true;
        public bool usesDefaultAttackAnim = true;
        public bool usesDefaultLeaveAnim = true;
        protected bool usesDefaultAttackSound = true;

        protected bool isCurrentEnemy = false;

        float[] sizes = { 1f, 0.8f, 0.6f, 0.4f, 0.3f };

        AudioClip acDefaultAttack;

        void Awake()
        {
        }

        protected void Initialize()
        {
            game = transform.parent.parent.parent.GetComponent<FacilityDash>();
            animator = transform.GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            spriteRenderer.enabled = false;
            animator.SetFloat(strSpeedString, game.gameSpeed / game.startingGameSpeed);

            acDefaultAttack = Pinto_ModBase.GetAudioClip(Pinto_ModBase.fdAudioPath + "67_knock (Player hit)");
        }

        protected void GameUpdate()
        {
            if (game.gameState == FDState.Lost)
            {
                return;
            }

            if (leaving)
            {
                if (leaveTimer > 0)
                {
                    leaveTimer -= Time.deltaTime;
                }
                else
                {
                    Dead();
                }
                return;
            }

            distanceDifference = distance - game.GetDistanceTraveled();
            if (distanceDifference < sizes.Length && distanceDifference >= 0)
            {
                spriteRenderer.enabled = true;
                transform.localScale = Vector3.one * sizes[distanceDifference];

                if (distanceDifference > 2)
                {
                    spriteRenderer.color = Color.black;
                }
                else
                {
                    spriteRenderer.color = Color.white;
                }
            }

            if (isCurrentEnemy)
            {
                if (attackTimer > 0)
                {
                    attackTimer -= Time.deltaTime;
                }
                else
                {
                    Attack();
                    attackTimer = attackSpeed / (3 / game.gameSpeed);
                }
            }

            oldDistanceDifference = distanceDifference;
        }

        protected void Dead()
        {
            dead.Invoke();
            Destroy(this);
        }

        public void IsInFront()
        {
            isCurrentEnemy = true;
            attackTimer = attackSpeed;
            game.PlaySound(acEntrance);
        }

        public void SetDistance(int newDistance)
        {
            distance = newDistance;
            spriteRenderer.enabled = true;
            transform.localScale = Vector3.zero;
            animator.SetFloat(strSpeedString, game.gameSpeed);
        }

        public virtual void Hurt()
        {
            if (leaving) return;
            if (usesDefaultHurtAnim)
            {
                animator.SetTrigger(strHurtString);
            }
        }

        public virtual void Leave()
        {
            if (usesDefaultLeaveAnim)
            {
                animator.SetTrigger(strLeaveString);
            }
            leaving = true;
            leaveTimer = leaveLengthTime;
        }

        public virtual void Attack()
        {
            if (usesDefaultAttackAnim)
            {
                animator.speed = 1 * (game.gameSpeed / 3);
                animator.SetTrigger(strAttackString);
            }
            if (usesDefaultAttackSound)
            {
                game.PlaySound(acDefaultAttack);
            }
            game.PlayerIsAttacked(attackDamage);
        }

        protected void InflictDamage(int amount)
        {
            game.PlayerIsAttacked(amount);
        }

        //returns true if dead
        public bool TakeDamage()
        {
            health--;
            Hurt();
            if (health > 0)
            {
                return false;
            }
            return true;
        }
    }
}