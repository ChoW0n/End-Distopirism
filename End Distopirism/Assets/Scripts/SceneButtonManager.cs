using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneButtonManager : MonoBehaviour
{
    public void OnGameStartButtonClick()
    {
        SceneManager.LoadScene("StageSelect");
    }

    public void OnStage1ButtonClick()
    {
        SceneManager.LoadScene("DeckBuilding");
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
