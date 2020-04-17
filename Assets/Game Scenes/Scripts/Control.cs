using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using Valve.VR;
 

public enum ActiveVRFamily {Vive, Oculus, None};

public class Control : MonoBehaviour
{
    public GameObject PC, Vive, Oculus;
    protected GameObject Player;
    public string detectedHeadset = "";
    public ActiveVRFamily activeVR;

    private void Awake()
    {
        
        detectedHeadset = XRDevice.model;


        if(detectedHeadset.ToLower().Contains("vive"))
        {
            //XRSettings.enable = true;
            Debug.Log("VIVE");
            activeVR = ActiveVRFamily.Vive;
            Player = Vive;
            // Instantiate Vive Player
            // Instantiate(myPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        }
        else if(detectedHeadset.ToLower().Contains("oculus"))
        {
            activeVR = ActiveVRFamily.Oculus;
            Player = Oculus;
            // Instantiate Oculus Player
        }
        else
        {
            Debug.Log("NONE");
            XRSettings.enabled = false;
            activeVR = ActiveVRFamily.None;
            Player = PC; 
            // Instantiate FPS Player
        }

        Player = Instantiate(Player, this.gameObject.transform);
        DontDestroyOnLoad(this.gameObject);
        DontDestroyOnLoad(Player);
    }
    public void NextScene()
    {
        SceneManager.LoadScene("ClientNetTest");    
    }

    public void QuitGame()
    {
        Debug.Log("Attempting Quit");
#if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }
}
