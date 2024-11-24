using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AnimTest : MonoBehaviour
{

    public Transform target;
    private NavMeshAgent agent;
    private Animator animator;

    void Awake() {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        agent.SetDestination(target.position);
        animator.SetFloat("Speed", agent.velocity.magnitude);
    }
}
