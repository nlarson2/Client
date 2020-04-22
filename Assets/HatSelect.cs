using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HatSelect : MonoBehaviour
{
    public int hatNo;
    public GameObject controller;
    private Control control;

    public GameObject bCap;
    public GameObject tHat;
    public GameObject cHat;

    private void Start()
    {
        control = controller.GetComponent<Control>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("HIT");
        if (collision.gameObject.tag == "Bullet")
        {
            control.characterSelect(hatNo);
            switch(hatNo)
            {
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    break;
                default:
                    break;
            }
        }
    }
    public void Collide()
    {
        control.characterSelect(hatNo);
    }
}
