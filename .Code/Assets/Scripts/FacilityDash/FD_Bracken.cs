using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PintoMod.Assets.Scripts.FacilityDash
{
    public class FD_Bracken : FD_Enemy
    {
        float leaveTimerMin = 1;
        float leaveTimerMax = 2;
        float leaveTime = 0;

        float maxStartingTime = 2;
        float staringTimer = 0;

        float postAggressionTimer = 0;
        float postAggressionTimerMax = 2;
        float postAggressionTimerMultiplier = 3f;
        float postAggressionScaleMultiplier = .2f;

        float postAttackedAttackSpeed = 2;

        public bool aggressive = false;
        bool oldAgressive = false;
        bool snappingNeck = false;

        AudioClip acAngered;

        void Awake()
        {
            Initialize();
            leaveTimer = Random.Range(leaveTimerMin, leaveTimerMax);
            EnemyName = "Bracken";
            Health = 6;
            AttackSpeeed = 3f;
            AttackDamage = 5;

            animator = GetComponent<Animator>();

            acEntrance = Pinto_ModBase.GetAudioClip(Pinto_ModBase.fdAudioPath + "monster sounds/Bracken found");
            acAngered = Pinto_ModBase.GetAudioClip(Pinto_ModBase.fdAudioPath + "monster sounds/Bracken Angered");
        }

        void Update()
        {
            base.GameUpdate();

            if (isCurrentEnemy)
            {
                if (aggressive)
                {
                    if (!oldAgressive)
                    {
                        game.PlaySound(acAngered);
                        oldAgressive = true;
                    }
                    postAggressionTimer += Time.deltaTime * postAggressionTimerMultiplier * (game.gameSpeed / game.startingGameSpeed);
                    if (!snappingNeck)
                    {
                        transform.localScale = Vector3.one + (Vector3.one * ((int)postAggressionTimer * postAggressionScaleMultiplier));

                        if (postAggressionTimer > postAggressionTimerMax * postAggressionTimerMultiplier)
                        {
                            snappingNeck = true;
                            animator.SetBool(strAttackModeString, snappingNeck);
                        }
                    }
                    else
                    {
                        if (postAggressionTimer > (postAggressionTimerMax * postAggressionTimerMultiplier) + postAggressionTimerMultiplier)
                        {
                            InflictDamage(5, false);
                            Dead();
                        }
                    }
                }

                if (!game.isHiding)
                {
                    if (staringTimer < maxStartingTime)
                    {
                        staringTimer += Time.deltaTime;
                    }
                    else
                    {
                        aggressive = true;
                    }
                }
                else
                {
                    if (leaveTimer > 0)
                    {
                        if (!aggressive)
                        {
                            leaveTimer -= Time.deltaTime;
                        }
                    }
                    else
                    {
                        if (!aggressive && !leaving)
                        {
                            Leave();
                        }
                    }
                }
            }
        }

        public override void Attack(bool isAttackBlockedByHiding)
        {
        }

        public override void Hurt()
        {
            if(!snappingNeck)
            {
                base.Hurt();
            }
            if (!aggressive)
            {
                aggressive = true;
            }
        }
    }
}