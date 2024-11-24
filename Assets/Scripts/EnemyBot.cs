using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBot : MonoBehaviour
{
    [Header("Relevant GameObjects")]
    public Transform target;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;
    UIHealthBar healthBar;
    private NavMeshAgent agent;
    private float health;
    RagdollManagerEnemy ragdollManager;
    public CharController playerScript;
    public GameObject weapon;

    [Header("Enemy Stats")]
    public float maxHealth = 100f;
    public float dieForce = 10f;

    [Header("Enemy States")]
    public float sightRange, attackRange, damageDealingRange;
    public bool playerInSightRange, playerInAttackRange;
    public bool dead;

    [Header("Patroling")]
    public bool patrolling;
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    [Header("Attacking")]
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    public GameObject projectile;
    private Animator animator;


    void Awake() {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.Find("Character").transform;
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        //playerScript = GetComponent<CharController>();
        ragdollManager = GetComponent<RagdollManagerEnemy>();
        healthBar = GetComponentInChildren<UIHealthBar>();
        var rigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach (var rigidbody in rigidbodies)
        {
            HitBox hitBox = rigidbody.gameObject.AddComponent<HitBox>();
            hitBox.bot = this;

        }
        health = maxHealth;
        healthBar.gameObject.SetActive(false);
    }


    // Update is called once per frame
    void Update()
    {
        if (!dead) {
        //Debug.Log("Enemy Living");
        //Check for sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (patrolling && !playerInSightRange && !playerInAttackRange) {
            ManageAnimationStages(0);
            Patroling(); }
        if (playerInSightRange && !playerInAttackRange) { 
            ManageAnimationStages(1);
            ChasePlayer();}
        if (playerInAttackRange && playerInSightRange) {AttackPlayer();}

        //Vector3 localVelocity = transform.InverseTransformDirection(agent.velocity);
        animator.SetFloat("SpeedZ", agent.velocity.magnitude);
        //animator.SetFloat("SpeedX", localVelocity.y);
        
        }else {
            agent.velocity = Vector3.zero;
        }
    }

    public void TakeDamage(float damage, Vector3 direction) {
        healthBar.gameObject.SetActive(true);
        animator.SetInteger("ImpactIndex", Random.Range(0, 2));
        animator.SetTrigger("Impact");
        health -= damage;
        healthBar.SetHealthBarSize(health/ maxHealth);
        if (health <= 0) {
            Debug.Log("Dead");
             HandleDeath(direction);
        }
    }

    private void HandleDeath(Vector3 direction) {
        ragdollManager.EnableRagdoll();
        direction.y = 1;
        ragdollManager.ApplyForce(direction * dieForce);
        dead = true;
        healthBar.gameObject.SetActive(false);
        //animator.SetInteger("DeathIndex", Random.Range(0, 2));
        //animator.SetTrigger("Death");
       
        //Invoke(nameof(DestroyWhenDead), 8f);
    }

    private void DestroyWhenDead() {
        Destroy(gameObject);
    }

    private void ManageAnimationStages(float alterLvl) {
        animator.SetFloat("alertLvl", alterLvl);
    }

    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //Walkpoint reached
        if (distanceToWalkPoint.magnitude <= agent.stoppingDistance)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        NavMeshHit hit;
        do {
            float randomZ = Random.Range(-walkPointRange, walkPointRange);
            float randomX = Random.Range(-walkPointRange, walkPointRange);

            walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);
        } while (!NavMesh.SamplePosition(walkPoint, out hit, walkPointRange, NavMesh.AllAreas));

        walkPoint = hit.position;
        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);

        transform.LookAt(player);

        if (!alreadyAttacked)
        {
            //Attack code here
            int damageIndex = Random.Range(0, 4);
            animator.SetInteger("AttackIndex", damageIndex);
            animator.SetTrigger("Attack");
            StartCoroutine(DelayedDamageCheck(damageIndex, 1f));
            Debug.Log("Attacking");

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), animator.GetCurrentAnimatorStateInfo(0).length);
        }
    }

    private void CheckDamageDealing(int damageIndex)
    {
        bool dealDamage = Physics.CheckSphere(weapon.transform.position, damageDealingRange, whatIsPlayer);
        int damage = 0;
        if (dealDamage)
        {
            switch (damageIndex)
            {
                case 0:
                    //Middle Attack
                    damage = 25;
                    break;
                case 1:
                    //Strong Attack
                    damage = 30;
                    break;
                case 2:
                    //Weak Attack
                    damage = 10;
                    break;
                case 3:
                    //Normal Attack
                    damage = 20;
                    break;
            }   
            Debug.Log("AI Dealing Damage: " + damage);
            playerScript.TakeDamage(damage);
        }   

    }

    private IEnumerator DelayedDamageCheck(int damageIndex, float delay)
    {
        yield return new WaitForSeconds(delay);
        CheckDamageDealing(damageIndex);
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
