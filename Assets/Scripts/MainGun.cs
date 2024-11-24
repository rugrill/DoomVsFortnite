using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainGun : MonoBehaviour
{
    public bool isShooting;
    public Transform cam;
    public Transform shootPoint;
    public float damage = 10f;
    public float timeBetweenShoots = 0.2f;
    public float nextShootTime = 0f;
    public ParticleSystem muzzleFlash;
    public TrailRenderer bulletTrail;
    public ParticleSystem hitEffect;
    private AudioSource audioSource;
    public AudioClip shootSound;
    //public GameObject HitEffect;

    
    // Start is called before the first frame update
    
    void Awake() {
        audioSource = GetComponent<AudioSource>();
    }   
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isShooting && Time.time > nextShootTime) {
            Shoot();
        }
    }

    private void Shoot() {
        Debug.Log("Shoting");

        //Bullet tracer
        var tracer = Instantiate(bulletTrail, shootPoint.position, Quaternion.identity);
        tracer.AddPosition(shootPoint.position);

        if(Physics.Raycast(cam.position, cam.forward , out RaycastHit hit, 100f))  {
            
            //Hit effect
            hitEffect.transform.position = hit.point;
            hitEffect.transform.forward = hit.normal;
            hitEffect.Emit(1);

            //Tracer hit effect position
            tracer.transform.position = hit.point;

            //Muzzle Flash effect
            muzzleFlash.Emit(1);


            //Collision impulse
            if(hit.rigidbody != null) {
                hit.rigidbody.AddForceAtPosition(cam.forward * 20, hit.point, ForceMode.Impulse);
            }

            var hitBox = hit.collider.GetComponent<HitBox>();
            if(hitBox) {
                hitBox.OnRaycastHit(this, cam.forward);
            }

            Debug.Log("Hit: " + hit.transform.name);
            EnemyBot enemy = hit.transform.GetComponent<EnemyBot>();
            if(enemy) {
                Debug.Log("Enemy Hit");
                //enemy.TakeDamage(5f);
            };
            
        }
        audioSource.PlayOneShot(shootSound);
        nextShootTime = Time.time + timeBetweenShoots;
       
    } 

    public void OnFire(InputAction.CallbackContext ctx) {
        if (ctx.started) {
           isShooting = true;
        } else if (ctx.canceled) {
            isShooting = false;
        }
    }
}
