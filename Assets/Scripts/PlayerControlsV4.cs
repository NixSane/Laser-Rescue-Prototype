using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Experimental.XR;

public class PlayerControlsV4 : MonoBehaviour
{
    CharacterController characterController;

    public BulletV3 bullet;

    // For rays and subsequent rays
    Ray laser_aim;
    Ray reflect_rays;
    LineRenderer laser_renderer;

    Vector3 mouse_pos;
    Vector3 reflect_dir;

    RaycastHit hit;

    public KeyCode aim_key = KeyCode.KeypadEnter;
    public KeyCode shoot_key = KeyCode.Space;

    // Limit to bounces
    float max_reflect_bounce = 10;

    // How far a ray can be drawn
    float distance = 100.0f;
    public float max_dist = 100.0f;
    public float player_speed = 10.0f;

    // New approach to laser reflections
    bool is_laser_going = false;

    List<Vector3> laser_points;
    List<Ray> rays_reflects;
    List<RaycastHit> reflect_hits;

    public int laser_bounces_limit = 10;

    Vector3 current_laser_origin;
    Vector3 current_laser_direction;

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();

        laser_points = new List<Vector3>(); // Things that will eventually be drawn
        rays_reflects = new List<Ray>();

        laser_renderer = GetComponent<LineRenderer>();
        distance = max_dist; // Default
        laser_renderer.positionCount = 2; // Default

        // reflect_rays = new Ray();
    }

    // Update is called once per frame
    void Update()
    {
        // Move player and set laser origin
        playerMove(player_speed);
        laser_renderer.SetPosition(0, transform.position);

        // Laser direction change
        mouse_pos = new Vector3(Input.mousePosition.x, transform.position.y, Input.mousePosition.y);
        current_laser_origin = transform.position;
        current_laser_direction = (mouse_pos - laser_renderer.GetPosition(0)).normalized;
        laser_aim = new Ray(laser_renderer.GetPosition(0), current_laser_direction);

        // Player looks in the same direction as laser
        transform.forward = current_laser_direction;

        // Shoot
        playerFire(shoot_key);

        // Check if the laser aim has 
        laserLength();

        // Toggle Aim enable reflective
        if (laserAim(aim_key))
        {
            // Check if the first laser has hit something
            if (Physics.Raycast(laser_aim, out hit, distance))
            {
                // Was it a mirror
                if (DidHitAMirror())
                {
                    // Get the reflective ray from first laser
                    current_laser_direction = Vector3.Reflect(current_laser_direction, hit.normal);
                    reflect_rays = new Ray(hit.point, current_laser_direction);

                    // Clear the points, add the first hit point
                    rays_reflects.Clear();
                    laser_points.Clear();
                    laser_points.Add(hit.point);

                    if (laser_renderer.positionCount < max_reflect_bounce)
                    {
                        rays_reflects.Add(reflect_rays);
                    }

                    Debug.DrawRay(reflect_rays.origin, reflect_rays.direction, Color.green);

                    // Has rays been maxed out?
                    if (rays_reflects.Count != max_reflect_bounce)
                    {
                        // Start looping for subsequent rays
                        for (int i = 0, j = 3; i < max_reflect_bounce; i++, j++)
                        {
                            if (Physics.Raycast(rays_reflects[i], out hit, max_dist))
                            {
                                // Did this hit a mirror?
                                if (DidHitAMirror())
                                {
                                    // Make sure it hasn't capped out
                                    if (rays_reflects.Count < laser_renderer.positionCount)
                                    {
                                        current_laser_direction = Vector3.Reflect(reflect_rays.direction, hit.normal);
                                        reflect_rays = new Ray(laser_points[i], rays_reflects[i].direction);
                                        rays_reflects.Add(reflect_rays);

                                        laser_points.Add(hit.point);

                                        laser_renderer.positionCount = j;
                                        laser_renderer.SetPosition(j - 1, hit.point);
                                    }
                                }
                                else
                                {
                                    is_laser_going = false;
                                }
                                Debug.DrawRay(rays_reflects[i].origin, rays_reflects[i].direction, Color.green);
                            }
                        }
                    }
                }
                else
                {
                    is_laser_going = false;
                }
            }


            // Clear the arrays
            laser_points.Clear();
            rays_reflects.Clear();
            laser_renderer.positionCount = 2;
            // Move Player and set the laser origin to new position
        }


            void playerMove(float speed)
            {
                float vertical_move = Input.GetAxis("Vertical");
                float horizontal_move = Input.GetAxis("Horizontal");

                Vector3 direction = new Vector3(horizontal_move, 0.0f, vertical_move).normalized;

                if (direction.magnitude != 0)
                {
                    characterController.Move(direction * speed * Time.deltaTime);
                }
            }

            void playerFire(KeyCode shoot_key)
            {
                if (Input.GetKeyDown(shoot_key))
                {
                    // characterController.enabled = false;
                    Instantiate(bullet);
                }
            }

            void laserLength()
            {
                // Check if the laser aim has 
                if (Physics.Raycast(laser_aim, out hit, distance))
                {
                    // Can this object be affected by the laser?
                    if (hit.collider.gameObject.layer != 2)
                    {
                        // Shorten current ray.
                        distance = (hit.point - laser_aim.origin).magnitude;
                        laser_renderer.SetPosition(1, hit.point);
                    }
                }
                else
                {
                    distance = max_dist;
                    laser_renderer.SetPosition(1, mouse_pos);
                    laser_aim = new Ray(laser_renderer.GetPosition(0), current_laser_direction);
                }
            }

            // Did the rays hit a mirror
            bool DidHitAMirror()
            {
                if (hit.collider.gameObject.layer == 8)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    /// <summary>
    /// Toggle laser on and off
    /// </summary>
    /// <param name="key"> Uses the key set to to trigger  </param>
    /// <returns></returns>
    bool laserAim(KeyCode key)
    {
        if (Input.GetKey(key))
        {
            if (!is_laser_going)
                is_laser_going = true;
            else
                is_laser_going = false;
        }
        return is_laser_going;
    }

    public Vector3 ReflectDirection
    {
        get { return current_laser_direction; }
    }

    public List<Ray> RayDeflections
    {
        get { return rays_reflects; }
    }
}