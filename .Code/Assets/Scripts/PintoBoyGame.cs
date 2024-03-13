using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PintoMod.Assets.Scripts
{
    public abstract class PintoBoyGame : MonoBehaviour
    {
        public PintoBoy pintoBoy;
        public PintoBoyCartridge cartridge;
        public bool isActive;
        public bool isPaused;
        public bool isInGame;
        public bool isHoldingButton;
        public bool wasInitialized = false;


        public virtual void ButtonPress() { }

        public virtual void ButtonRelease(float timeHeld) { }

        public virtual void GameUpdate() { }

        public virtual void UnPause() { }
        public virtual void Pause() { }

        public virtual void TurnedOff() { }
        public virtual void TurnedOn() { }

        public virtual void InitializeObjects(Transform gameRoot) 
        {
            Debug.Log("Intiailizing PintoBoyGame: "+this.name);
            wasInitialized = true;
        }

        public virtual void InsertedIntoPintoBoy(PintoBoy pintoBoy, Transform gameRoot, PintoBoyCartridge cartridge)
        {
            try
            {
                EnableChildren();
                Debug.Log("Inserted Game");
                this.pintoBoy = pintoBoy;
                this.cartridge = cartridge;
                transform.parent = gameRoot;
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                transform.localScale = Vector3.one;
                if (!wasInitialized)
                {
                    Debug.Log("Initializing Game that was Inserted: "+ gameRoot.name);
                    
                    InitializeObjects(transform);
                }
                else
                {
                    Debug.Log("already initialized??");
                }
            }
            catch(Exception e)
            {
                Debug.Log($"{e}: PintoBoyGame.InsertedIntoPintoBoy() failed: {pintoBoy}");
            }
        }

        public virtual void TakenOutPintoBoy()
        {
            pintoBoy = null;
            DisableChildren();
        }

        public virtual void DisableChildren()
        {
            for(int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        public virtual void EnableChildren()
        {
            for(int i =0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
        }

        public virtual void StopSounds()
        {
            pintoBoy.StopSounds();
        }
        public void PlaySound(AudioClip clip)
        {
            pintoBoy.PlaySound(clip);
        }
    }
}
