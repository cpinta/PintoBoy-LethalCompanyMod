using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PintoMod.Assets.Scripts.FacilityDash
{
    public class FD_LootBug : FD_Enemy
    {
        float leaveTimerMin = 3;
        float leaveTimerMax = 5;
        float leaveTime = 0;

        public bool aggressive = false;
        bool firstAttack = true;

        AudioClip acAngered;

        void Awake()
        {
            Initialize();
            leaveTimer = 3 / (game.gameSpeed / game.startingGameSpeed);
            EnemyName = "Loot Bug";
            Health = 2;
            AttackSpeeed = 2;

            acEntrance = Pinto_ModBase.GetAudioClip(Pinto_ModBase.fdAudioPath + "monster sounds/loot bug walk");
            acAngered = Pinto_ModBase.GetAudioClip(Pinto_ModBase.fdAudioPath + "monster sounds/loot bug dead");
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
            if (firstAttack)
            {
                game.PlaySound(acAngered);
                firstAttack = false;
            }
            base.Attack(true);
        }

        public override void Hurt()
        {
            base.Hurt();
            if (!aggressive)
            {
                aggressive = true;
                base.animator.SetBool(strAttackModeString, true);
                Attack(true);
            }
        }
    }
}