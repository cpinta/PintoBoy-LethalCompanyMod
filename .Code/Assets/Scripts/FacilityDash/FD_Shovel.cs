using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace PintoMod.Assets.Scripts.FacilityDash
{
    public class FD_Shovel : MonoBehaviour
    {
        Animator animator;
        public SpriteRenderer spriteRenderer;
        public UnityEvent hit;
        public UnityEvent dead;

        // Start is called before the first frame update
        void Awake()
        {
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Swing()
        {
            hit.Invoke();
        }

        public void TurnOff()
        {
            spriteRenderer.enabled = false;
        }

        public void TurnOn()
        {
            spriteRenderer.enabled = true;
        }

        public void Dead()
        {
            dead.Invoke();
        }
    }
}