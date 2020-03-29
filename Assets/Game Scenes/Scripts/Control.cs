using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
 
public class Control : MonoBehaviour
{
    public GameObject Player;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    public void NextScene()
    {
        Destroy(Player);
        SceneManager.LoadScene("Test Room");    
    }
}
