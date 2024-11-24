using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public string gameSceneName = "EndScene";
   
   
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Keeps the GameManager across scenes
        }
        else
        {
            Destroy(gameObject); // Ensures there's only one instance
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPlayerDeath()
    {
        // Load the game over scene
        SceneManager.LoadScene(gameSceneName);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
