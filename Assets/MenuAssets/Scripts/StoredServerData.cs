using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StoredServerData : MonoBehaviour
{
    public Button button;
    public Text domain;
    public string domainAddress;
    public Text port;
    public int portNum;
    // Start is called before the first frame update
    void Start()
    {
        
        DontDestroyOnLoad(this.gameObject);
        Button buttonListener = button.GetComponent<Button>();
        buttonListener.onClick.AddListener(OnBtnClick);
    }
    void OnBtnClick()
    {
        try
        {
            Debug.Log(domain.text);
            Debug.Log(port.text);
            if (port.text == "" || domain.text == "")
            {
                Debug.Log("ERROR NULL VALUE");
                return;
            }

            portNum = Int32.Parse(port.text);
            domainAddress = domain.text;

            SceneManager.LoadScene(1);
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
    }
    public void Destory()
    {
        Destroy(this.gameObject);
    }
}
