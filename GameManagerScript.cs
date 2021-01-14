using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerScript : MonoBehaviour
{
    GameObject Pellets;
    public int maxPellets;
    public int currentPellets;
    public int currentLevel = 1;

    public float MaxFrightenedTime = 15.0f;
    public float CurrentFrightenedTime = 0.0f;

    private static GameManagerScript gameManagerInstance;
    public GameObject blinky;
    GameObject inky;
    GameObject pinky;
    GameObject clyde;
    public bool isFrightened = false;


    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (gameManagerInstance == null)
        {
            gameManagerInstance = this;
        }
        else
        {
            DestroyObject(gameObject);
        }
    }

    private void Update()
    {
        if (CurrentFrightenedTime > 0.0f)
        {
            CurrentFrightenedTime -= Time.deltaTime;
        }
        else
        {
            if (isFrightened)
            {
                ExitFrightenedMode();
            }
        }
    }

    void Start()
    {
        Pellets = GameObject.Find("PelletsContainer");
        currentPellets = 0;
        maxPellets = Pellets.transform.childCount;
        AssignGhosts();
    }

    void AssignGhosts()
    {
        blinky = GameObject.Find("Blinky");
        pinky = GameObject.Find("Pinky");
        inky = GameObject.Find("Inky");
        clyde = GameObject.Find("Clyde");
    }

    IEnumerator DelayedAssignGhosts()
    {
        yield return new WaitForSeconds(1);

        blinky = GameObject.Find("Blinky");
        pinky = GameObject.Find("Pinky");
        inky = GameObject.Find("Inky");
        clyde = GameObject.Find("Clyde");
    }

    public void PointGet()
    {
        currentPellets++;
        if (currentPellets >= maxPellets)
        {
            NextLevel();
        }
    }

    public void NextLevel()
    {
        currentLevel++;
        ResetLevel();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        MaxFrightenedTime -= 2;

    }

    public void ResetGame()
    {
        ResetLevel();
        currentLevel = 1;
        CurrentFrightenedTime = 0;
        MaxFrightenedTime = 15.0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        

    }

    public void ResetLevel()
    {
        currentPellets = 0;
        isFrightened = false;
        StartCoroutine(DelayedAssignGhosts());
    }

    public int getCurrentLevel() { return currentLevel; }

    public void EnterFrightenedMode()
    {
        if (MaxFrightenedTime > 0f)
        {
            blinky.GetComponent<Pathfinding>().EnterFrightened();
            inky.GetComponent<Pathfinding>().EnterFrightened();
            pinky.GetComponent<Pathfinding>().EnterFrightened();
            clyde.GetComponent<Pathfinding>().EnterFrightened();

            CurrentFrightenedTime = MaxFrightenedTime;
            isFrightened = true;
        }
        else
        {
            Debug.Log("Too high level for that powerup!");
        }

    }

    public void ExitFrightenedMode()
    {
        if (CurrentFrightenedTime < 0f)
        {
            isFrightened = false;
            blinky.GetComponent<Pathfinding>().ExitFrightened();
            inky.GetComponent<Pathfinding>().ExitFrightened();
            pinky.GetComponent<Pathfinding>().ExitFrightened();
            clyde.GetComponent<Pathfinding>().ExitFrightened();
        }
    }
}
