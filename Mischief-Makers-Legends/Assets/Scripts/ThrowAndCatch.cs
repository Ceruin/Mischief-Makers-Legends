using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ThrowAndCatch : MonoBehaviour
{
    public Transform throwOrigin; // the point where the object is thrown from
    public float throwForce = 10f; // the force with which the object is thrown
    public LayerMask catchLayer; // the layer(s) that the object can be caught on

    private Rigidbody rb;
    private Collider collider;
    private bool isHeld = false;
    private Vector3 holdOffset;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
    }

    private void Update()
    {
        if (!isHeld && Input.GetKeyDown(KeyCode.E))
        {
            Throw();
        }
        else if (isHeld && Input.GetKeyDown(KeyCode.E))
        {
            Drop();
        }
        else if (isHeld && Input.GetMouseButtonDown(0))
        {
            Catch();
        }
    }

    private void Throw()
    {
        rb.isKinematic = false;
        rb.AddForce(throwOrigin.forward * throwForce, ForceMode.Impulse);
    }

    private void Drop()
    {
        isHeld = false;
        rb.isKinematic = false;
        collider.enabled = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    private void Catch()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 1f, catchLayer);

        if (hits.Length > 0)
        {
            isHeld = true;
            rb.isKinematic = true;
            collider.enabled = false;
            holdOffset = transform.position - hits[0].transform.position;
        }
    }

    private void FixedUpdate()
    {
        if (isHeld)
        {
            rb.MovePosition(transform.position + holdOffset);
        }
    }
}

