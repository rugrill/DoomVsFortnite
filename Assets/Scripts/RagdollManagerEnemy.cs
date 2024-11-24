using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollManagerEnemy : MonoBehaviour
{

     Rigidbody[] rigidbodies;
     Animator animator;
    // Start is called before the first frame update
    void Start()
    {
         rigidbodies = GetComponentsInChildren<Rigidbody>();
         animator = GetComponentInChildren<Animator>();

         DeactivateRagdoll();
    }

    public void DeactivateRagdoll()
    {
       
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = true;
        }
        animator.enabled = true;
    }
    public void EnableRagdoll()
    {
        
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = false;
        }
        animator.enabled = false;
    }

    public void ApplyForce(Vector3 force)
    {
        var rigidbody = animator.GetBoneTransform(HumanBodyBones.Hips).GetComponent<Rigidbody>();
        rigidbody.AddForce(force, ForceMode.VelocityChange);
    }
}
