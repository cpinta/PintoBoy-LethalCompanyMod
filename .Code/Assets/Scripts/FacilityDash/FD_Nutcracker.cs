using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PintoMod.Assets.Scripts.FacilityDash
{
    public class FD_Nutcracker : FD_Enemy
    {
        float leaveTimerMin = 5;
        float leaveTimerMax = 10;
        float leaveTime = 0;

        public bool aggressive = false;
        bool firstAttack = true;

        AudioClip acAngered;
        AudioClip acShotgun;

        void Awake()
        {
            Initialize();
            leaveTimer = 3 / (game.gameSpeed / game.startingGameSpeed);
            EnemyName = "Nutcracker";
            Health = 5;
            AttackSpeeed = 2.5f;

            animator = GetComponent<Animator>();

            acEntrance = Pinto_ModBase.GetAudioClip(Pinto_ModBase.fdAudioPath + "monster sounds/nutcracker entrance");
            acAngered = Pinto_ModBase.GetAudioClip(Pinto_ModBase.fdAudioPath + "monster sounds/nutcracker mad");
            acShotgun = Pinto_ModBase.GetAudioClip(Pinto_ModBase.fdAudioPath + "monster sounds/shotgun");
            usesDefaultAttackSound = false;
        }

        void LateUpdate()
        {
            base.GameUpdate();
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

        public override void Attack(bool isAttackBlockedByHiding)
        {
            if (!aggressive) return;
            animator.SetBool(strAttackString, true);

            AttackDamage = Random.Range(1, 5);
            if (firstAttack)
            {
                game.PlaySound(acAngered);
                firstAttack = false;
            }
            game.PlaySound(acShotgun);
            base.Attack(true);
        }

        public override void Hurt()
        {
            base.Hurt();
            if (!aggressive)
            {
                aggressive = true;
            }
        }
    }
}