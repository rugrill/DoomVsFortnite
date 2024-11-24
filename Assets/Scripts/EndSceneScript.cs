using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndSceneScript : MonoBehaviour
{
    public string gameSceneName = "SampleScene";
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnTryAgainButtonClick() {
        SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
    }
}
