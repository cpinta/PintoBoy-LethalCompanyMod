using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PintoMod.Assets.Scripts.FacilityDash
{
    public class FD_LootBug : FD_Enemy
    {
        public bool aggressive = false;
        bool firstAttack = true;

        public AudioClip acAngered;

        void Awake()
        {
            Initialize();
        }

        void LateUpdate()
        {
            base.GameUpdate();
            if(game != null && leaveTimer == 0)
            {
                leaveTimer = 3 / (game.gameSpeed / game.startingGameSpeed);
            }




            if (leaveTimer > 0)
            {
                if (!aggressive)
                {
                    leaveTimer -= Time.deltaTime;
                    transform.localPosition = Vector3.zero;
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

        public override void Initialize()
        {
            base.Initialize();

            EnemyName = "Loot Bug";
            Health = 2;
            AttackSpeeed = 2;
            acEntrance = Pinto_ModBase.GetAudioClip(Pinto_ModBase.fdAudioPath + "monster sounds/loot bug walk");
            acAngered = Pinto_ModBase.GetAudioClip(Pinto_ModBase.fdAudioPath + "monster sounds/loot bug dead");
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