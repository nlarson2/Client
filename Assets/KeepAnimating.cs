using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepAnimating : MonoBehaviour
{
    Animation a;
    private void Start()
    {
        a = this.GetComponent<Animation>();
    }
    // Update is called once per frame
    void Update()
    {
        if (!a.isPlaying)
            a.Play();
    }
}
