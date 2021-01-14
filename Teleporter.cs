using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public Transform teleportTarget;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Teleporting!");
        if(other.gameObject.name == "Player")
        {
            other.transform.position = teleportTarget.position;
        }else if(other.gameObject.name == "PlayerBody")
        {
            other.transform.parent.transform.position = teleportTarget.position;
        }
        else
            other.transform.position = teleportTarget.position;

        
    }
}
