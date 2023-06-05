using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public Animator animator;

    [SerializeField] private int maxHealth = 100;
    int currentHealth;
    //public int damage = 20;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;      // set health
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        animator.SetTrigger("Hurt");        // play Hurt animation


        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Enemy died, RIP.");

        animator.SetBool("IsDead", true);       // play death animation
        
        GetComponent<Collider2D>().enabled = false;      // disable collider
        this.enabled = false;                           // disable enemy script
    }

    public void DestroyObject()
    {
        Destroy(gameObject);    //  INHERITED FROM MONOBEHAVIOUR
    }
}
