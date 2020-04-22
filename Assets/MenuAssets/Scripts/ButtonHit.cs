using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonHit : MonoBehaviour
{

    public string scene;
    public GameObject controller;
    private Control control;

    private void Start()
    {
        control = controller.GetComponent<Control>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("HIT");
        if(collision.gameObject.tag == "Bullet")
        {
            if (scene == "Game")
            {
                control.NextScene();
            }
            if(scene == "Quit")
            {
                control.QuitGame();
            }
        }
    }
    public void Collide()
    {
        if (scene == "Game")
        {
            control.NextScene();
        }
        if (scene == "Quit")
        {
            control.QuitGame();
        }
    }
}
