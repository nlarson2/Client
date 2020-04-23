//#define VR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class SimpleShootPC : MonoBehaviour
{

    public SteamVR_Action_Boolean fireAction;
    public GameObject bulletPrefab;
    public GameObject casingPrefab;
    public GameObject muzzleFlashPrefab;
    public Transform barrelLocation;
    public Transform casingExitLocation;

    public float fireRate = 0.5f;
    public float nextFire = 0.0f;

    public float shotPower = 100f;

    private Interactable interactable;
    private Animator animator;

    void Start()
    {
        interactable = GetComponent<Interactable>();
        animator = GetComponent<Animator>();
        if (barrelLocation == null)
            barrelLocation = transform;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0) && Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;
            animator.Play("Shooting",0,0);
        }
    }

    void Shoot()
    {
        GameObject tempFlash;
        tempFlash = Instantiate(muzzleFlashPrefab, barrelLocation.position, barrelLocation.rotation);
        RaycastHit hit;
        Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 100.0f);
        GameObject hitObj = hit.collider.gameObject;
        if(hitObj.tag == "hat")
        {
            hitObj.GetComponent<HatSelect>().Collide();
        }
        if (hitObj.tag == "button")
        {
            hitObj.GetComponent<ButtonHit>().Collide();
        }

        Destroy(tempFlash, 0.5f);
        //Instantiate(casingPrefab, casingExitLocation.position, casingExitLocation.rotation).GetComponent<Rigidbody>().AddForce(casingExitLocation.right * 100f);

    }

    void CasingRelease()
    {
        GameObject casing;
        casing = Instantiate(casingPrefab, casingExitLocation.position, casingExitLocation.rotation) as GameObject;
        casing.GetComponent<Rigidbody>().AddExplosionForce(550f, (casingExitLocation.position - casingExitLocation.right * 0.3f - casingExitLocation.up * 0.6f), 1f);
        casing.GetComponent<Rigidbody>().AddTorque(new Vector3(0, Random.Range(100f, 500f), Random.Range(10f, 1000f)), ForceMode.Impulse);
    }


}
