using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetPoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.gameObject.GetComponent<Pathfinding>() != null)
            other.transform.gameObject.GetComponent<Pathfinding>().StopEaten();
    }
}
