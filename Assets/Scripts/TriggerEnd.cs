using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerEnd : MonoBehaviour
{
    public Canvas game_end;

    private void OnTriggerEnter(Collider other)
    {
        GameObject obj = other.gameObject;

        if (obj.name == "Bullet" || obj.tag == "Bullet")
        {
            game_end.enabled = false;
            gameObject.SetActive(false);

            Object.Destroy(gameObject);
        }
    }
}
