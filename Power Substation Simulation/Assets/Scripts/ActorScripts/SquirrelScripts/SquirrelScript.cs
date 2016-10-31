﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;


public class SquirrelScript:MonoBehaviour, Interactable
{
    public Animator animator;

    Rigidbody[] bodies;
    public float speed = 1.0f;
    public float climbingSpeed = 2.0f;


    protected Rigidbody rigidBody;
    public bool ragdollState = false;

    List<Collision> collisionList = new List<Collision>();


    List<Plane> collisionPlanes = new List<Plane>();

    public GameObject leftHand;
    public GameObject rightHand;


    public GameObject pathContainer = null;
    protected List<Vector3> path;
    protected int currentPathNode = 0;



    protected bool pinned = false;
    public bool alive = true;


    //SortedDictionary<Collider, Collision> collisionDictionary = new SortedDictionary<Collider, Collision>();
    Dictionary<Collider, Collision> collisionDictionary = new Dictionary<Collider, Collision>();

    public void Start()
    {
        extractPathVectors();
        rigidBody = GetComponent<Rigidbody>();//get the rigid body for the character
        bodies = GetComponentsInChildren<Rigidbody>();
        


        animator = GetComponent<Animator>();
        animator.SetBool("Walking", true);
        animator.SetBool("Climbing", false);
        animator.SetBool("Dead", false);
        animator.SetBool("Grabbing", false);



        setRagdollState(ragdollState);
    }



    public void Update()
    {
       // Vector3 direction = transform.position+new Vector3(0,0,speed);
        //Vector3 velocity = new Vector3(0,0,speed)*Time.deltaTime;

        Vector3 direction = path[currentPathNode] - transform.position;
        Vector3 velocity = direction.normalized * speed * Time.deltaTime;
        velocity.y = 0;
        if (ragdollState == false)
             move(velocity);

        if (Vector3.Distance(transform.position, path[currentPathNode]) < 1.5f)
        {
            if (currentPathNode < path.Count - 1)
                currentPathNode++;

        }

        //collision.contacts[0];


    }


    public void move(Vector3 velocity)
    {
        //get all registered collisions
        List<Collision> collisions = new List<Collision>();
        collisions.AddRange(collisionDictionary.Values);

        List<Plane> velocityIntersectsPlane = new List<Plane>();
        bool intersected = false;
        foreach( Collision c in collisions)
        {
            //for simplicity, and speed we'll only check the first registered collision point per collision,
            //since for complex objects this 
            Plane collisionPlane = new Plane(c.contacts[0].normal, c.contacts[0].point);
            Ray velocityRay = new Ray(transform.position, velocity.normalized);
            float distance = 0;
            if (collisionPlane.Raycast(velocityRay, out distance))
            {
                if (distance >= .8f)
                    continue;
                //velocity = Vector3.zero;
                velocity.y += climbingSpeed*Time.deltaTime;
                Vector3 vQ = transform.position + velocity;
                Vector3 vPQ =vQ- c.contacts[0].point;
                float dot = Vector3.Dot(vPQ, collisionPlane.normal.normalized);
                Vector3 vQN = collisionPlane.normal.normalized * dot;

                Vector3 vAnswer = vQ - vQN;
                velocity = vAnswer - transform.position;
                
                intersected = true;
                break;
            }
        }
        


        Vector3 newPosition = transform.position + velocity;
        rigidBody.position = newPosition;

        float turnSpeed = 30;
        velocity.y = 0;
        Quaternion dirQ = Quaternion.LookRotation(velocity);
        Quaternion slerp = Quaternion.Slerp(transform.rotation, dirQ, velocity.magnitude * turnSpeed * Time.deltaTime);
        rigidBody.MoveRotation(slerp);
        
       // rigidBody.MovePosition(newPosition);


    }






    void OnCollisionEnter(Collision collision)
    {
        
        ContactPoint cp = collision.contacts[0];
        collisionDictionary.Add(collision.collider, collision);
        if (collision.gameObject.tag == "Transformer")
        {
            
        }



    }
    void OnCollisionStay(Collision collision)
    {
        if (collisionDictionary.ContainsKey(collision.collider))
        {
            collisionDictionary[collision.collider] = collision;
        }
        else
            Debug.Log("Could not find in dictionary");
    }
    void OnCollisionExit(Collision collision)
    {

        if (collisionDictionary.ContainsKey(collision.collider))
        {
            collisionDictionary.Remove(collision.collider);
           



        }
        else
            Debug.Log("trying to remove non-existant key");

        
    }



    void OnTriggerEnter(Collider other)
    {
        if (alive)
        {
            alive = false;
            setRagdollState(true);
            setHandLocked(true);
            pinned = true;
        }
       


    }
    void OnTriggerStay(Collider other)
    {

    }
    void OnTriggerExit(Collider other)
    {

    }


    public void setRagdollState(bool newValue)
    {
        Debug.Log("Setting ragdoll to: " + newValue);
        ragdollState = newValue;
        animator.enabled = !newValue;


        CapsuleCollider cc = GetComponent<CapsuleCollider>();
        cc.isTrigger = newValue;


        //For each of the components in the array, treat the component as a Rigidbody and set its isKinematic property
        foreach (Rigidbody rb in bodies)
        {

            rb.isKinematic = !newValue;
            rb.detectCollisions = newValue;
            
            

        }


        rigidBody.isKinematic = newValue;
        
        rigidBody.detectCollisions = true;
    }

    void setHandLocked(bool newHandLockedValue)
    {
        Rigidbody leftHandBody = leftHand.GetComponent<Rigidbody>();
        leftHandBody.isKinematic = newHandLockedValue;

        Rigidbody rightHandBody = rightHand.GetComponent<Rigidbody>();
        rightHandBody.isKinematic = newHandLockedValue;

    }


    void extractPathVectors()
    {
        path = new List<Vector3>();
        if (pathContainer != null)
        {
            foreach (Transform child in pathContainer.transform)
            {
                //child is your child transform
                path.Add(child.transform.position);
            }
        }
    }


    public void setNewPath(GameObject newPathContainer)
    {
        pathContainer = newPathContainer;
        extractPathVectors();
        currentPathNode = 0;
    }

    public void interact(GameObject interactor)
    {
        if (pinned==true && interactor.tag == "Scraper")
        {
            setHandLocked(false);
            pinned = false;
        }
    }

    public void displayInteractionMessage(GameObject interactor)
    {
        if (pinned == true)
        {
            GUI.color = Color.white;
            GUI.Box(new Rect(20, 20, 200, 55), "get the scrapper, press e to remove");
        }
    }
}

