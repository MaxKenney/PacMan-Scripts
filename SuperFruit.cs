using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperFruit : MonoBehaviour
{
    public GameManagerScript gameManager;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManagerScript>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            gameManager.EnterFrightenedMode();
            gameObject.SetActive(false);
        }
    }
}
