using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class eggytime : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        this.gameObject.transform.localScale += new Vector3(25, 25, 25);
    }
}
