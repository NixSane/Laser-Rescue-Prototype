using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletV3 : MonoBehaviour
{
    Rigidbody rb;
    float numBounces = 10;
    int bounces = 0;
    public float speed = 5.0f;

    public GameObject player;
    CharacterController player_controller;
    public Transform gun;
    PlayerControlsV4 player_controls;


    // Start is called before the first frame update
    void Start()
    {
        player_controls = player.GetComponent<PlayerControlsV4>();
        player_controller = player.GetComponent<CharacterController>();
        gun = GameObject.Find("Laser Gun").transform;
        rb = GetComponent<Rigidbody>();
        transform.position = gun.position + (gun.forward * 2);
        transform.up = gun.forward;

        rb.angularVelocity = Vector3.zero;
        rb.velocity = Vector3.zero;

        numBounces = player_controls.RayDeflections.Count;
    }

    // Update is called once per frame
    void Update()
    {
        player_controller.enabled = true;
        rb.AddForce(transform.up * speed * Time.deltaTime, ForceMode.VelocityChange);
    }

    private void OnCollisionEnter(Collision collision)
    {

        ContactPoint[] contact = collision.contacts;
        
        if (bounces < numBounces)
        {
            Vector3 deflect = Vector3.Reflect(rb.velocity, contact[0].normal);

            //// dot *= 2;
            // Vector3 reflection = contact.normal * dot;

            // reflection = reflection + transform.forward;
            rb.angularVelocity = Vector3.zero;
            rb.velocity = Vector3.zero;
            rb.velocity = deflect.normalized * 10.0f;
            bounces++;
        }
    }
}
