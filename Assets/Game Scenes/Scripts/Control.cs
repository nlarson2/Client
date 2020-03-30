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
        DontDestroyOnLoad(Player);
    }
    public void NextScene()
    {
        SceneManager.LoadScene("ClientNetTest");    
    }
}
