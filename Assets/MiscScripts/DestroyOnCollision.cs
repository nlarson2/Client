using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnCollision : MonoBehaviour
{
    public GameObject muzzleFlashPrefab;
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
        Vector3 pos = this.gameObject.transform.position;
        GameObject tempFlash;
        tempFlash = Instantiate(muzzleFlashPrefab, pos, Quaternion.identity);
       // tempFlash.transform.localScale *= 10;
        Destroy(this.gameObject);
    }
}
