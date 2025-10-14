using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadLevel : MonoBehaviour
{
    public void LoadLevelByBuildIndex(int index)
    {
        SceneManager.LoadScene(index);
    }
    
    public void LoadLevelByName(String levelName)
    {
        SceneManager.LoadScene(levelName);
    }
}
