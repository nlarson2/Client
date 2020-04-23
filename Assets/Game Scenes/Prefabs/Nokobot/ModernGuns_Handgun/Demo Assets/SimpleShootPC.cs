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

    float fireRate = 0.5f;
    float nextFire = 0.5f;

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
        //  GameObject bullet;
        //  bullet = Instantiate(bulletPrefab, barrelLocation.position, barrelLocation.rotation);
        // bullet.GetComponent<Rigidbody>().AddForce(barrelLocation.forward * shotPower);
        print("shoot");
        GameObject tempFlash;
        //Instantiate(bulletPrefab, barrelLocation.position, barrelLocation.rotation).GetComponent<Rigidbody>().AddForce(barrelLocation.forward * shotPower);
        tempFlash = Instantiate(muzzleFlashPrefab, barrelLocation.position, barrelLocation.rotation);
        RaycastHit hit;
        Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 100.0f);
        //Debug.DrawRay(shootMsg.position, fwd * 20, Color.green, 5, false);
        //Debug.Log(string.Format("hit something? {0}", hit.transform.name));
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
