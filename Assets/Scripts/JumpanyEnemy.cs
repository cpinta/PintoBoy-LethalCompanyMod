using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class JumpanyEnemy : MonoBehaviour
{
    public float speed = 5f;
    float startX;
    float distance;
    public PintoBoy pintoBoy;
    public UnityEvent<JumpanyEnemy> onDeath;

    // Start is called before the first frame update
    void Awake()
    {
        startX = this.transform.position.x;
        distance = startX * 2;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position += Vector3.left * speed * Time.deltaTime;
        if (this.transform.position.x < startX - distance)
        {
            onDeath.Invoke(this);
            Destroy(this.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            pintoBoy.Die();
        }
    }
}
