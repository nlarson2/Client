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
    public int playerType;
    public int personType;
    public string detectedHeadset = "";
    public ActiveVRFamily activeVR;

    public GameObject BrandonH;
    public GameObject BrandonB;
    public GameObject DOMINANT;

    private void Awake()
    {
        
        detectedHeadset = XRDevice.model;


        if(detectedHeadset.ToLower().Contains("vive"))
        {
            //XRSettings.enable = true;
            //Debug.Log("VIVE");
            activeVR = ActiveVRFamily.Vive;
            Player = Vive;
            playerType = 2;
            // Instantiate Vive Player
            // Instantiate(myPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        }
        else if(detectedHeadset.ToLower().Contains("oculus"))
        {
            activeVR = ActiveVRFamily.Oculus;
            Player = Oculus;
            playerType = 2;
            // Instantiate Oculus Player
        }
        else
        {
            //Debug.Log("NONE");
            XRSettings.enabled = false;
            playerType = 1;
            activeVR = ActiveVRFamily.None;
            Player = PC;
            switch (this.personType)
            {
                case 1:
                    Player = BrandonH;
                    break;
                case 2:
                    Player = BrandonB;
                    break;
                case 3:
                    Player = DOMINANT;
                    break;
                default:
                    break;
            }
            // Instantiate FPS Player
        }

        Player = Instantiate(Player, this.gameObject.transform);
        Player.GetComponent<LocalPlayer>().personType = this.personType;
        DontDestroyOnLoad(this.gameObject);
        DontDestroyOnLoad(Player);
    }
    public void NextScene()
    {
        SceneManager.LoadScene("ClientNetTest");    
    }

    public void QuitGame()
    {
        //Debug.Log("Attempting Quit");
#if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }
}
