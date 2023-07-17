using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum State { Newspaper, Wood, Cement }
public class PlayerBehaviour : MonoBehaviour
{
    [Header("Movement Attributes")]
    public float movementSpeed = 50f;
    public float maxSpeedLimit = 4f; 
    private bool canMove = true;
    private float horizontalAxis;
    private float verticalAxis;
    private Rigidbody rigidBody;

    [Header("Wind Area Moveement")]
    public bool inWindArea = false;
    public GameObject WindArea;

    [Header("State Attributes")]
    public State playerState;
    public Material woodMaterial;
    public Material cementMaterial;
    public Material paperMaterial;

    [Header("Push Attribute")]
    private bool canPush=true;

    [Header("GroundStateAttributes")]
    [SerializeField] private bool isGrounded;
    public float maxRayDistance;
    public LayerMask layerMask;

    [Header("Game Camera")]
    public Camera gameCamera;
    private CameraBehaviour cameraBehaviour;


    // Start is called before the first frame update
    void Start()
    {
        rigidBody = this.GetComponent<Rigidbody>();
        cameraBehaviour = gameCamera.GetComponent<CameraBehaviour>();
        ChangeState(playerState);
    }

    // Update is called once per frame
    private void Update()
    {
        RayCasting();
        //Clamp if the speed exceeds the maxSpeedLimit
        if (rigidBody.velocity.magnitude > maxSpeedLimit && isGrounded)
        {
            rigidBody.velocity=Vector3.ClampMagnitude(rigidBody.velocity, maxSpeedLimit);
        }
    }
    private void FixedUpdate()
    {
        
        //Player Movement Behaviour
        Movement();
        //In Wind Zone
        WindMovement();
    }

    void Movement()
    {
        //if the player can move then add force based on Input Direction
        if (canMove)
        {
            if (cameraBehaviour.GetCameraState() == CameraState.Front)
            {
                horizontalAxis = Input.GetAxis("Horizontal");
                verticalAxis = Input.GetAxis("Vertical");
            }
            else if (cameraBehaviour.GetCameraState() == CameraState.Back)
            {
                horizontalAxis = -Input.GetAxis("Horizontal");
                verticalAxis = -Input.GetAxis("Vertical");
            }
            else if (cameraBehaviour.GetCameraState() == CameraState.Right)
            {
                horizontalAxis = -Input.GetAxis("Vertical");
                verticalAxis = Input.GetAxis("Horizontal");
            }
            else if (cameraBehaviour.GetCameraState() == CameraState.Left)
            {
                horizontalAxis = Input.GetAxis("Vertical");
                verticalAxis = -Input.GetAxis("Horizontal");
            }
            Vector3 movementForce = new Vector3(horizontalAxis * movementSpeed * Time.fixedDeltaTime, 0f, verticalAxis * movementSpeed * Time.fixedDeltaTime);
            rigidBody.AddForce(movementForce);
        }
    }

    void WindMovement()
    {
        //if the player enters the wind Area
        if (inWindArea)
        {
            if(playerState == State.Newspaper)
            {
                WindArea windArea = WindArea.GetComponent<WindArea>();
                rigidBody.AddForce(windArea.direction * windArea.strength);
            }
        }
    }

    void ChangeState(State newState)
    {
        // Change the State of the Player based on the Tranistion Plate
        playerState = newState;
        switch (newState)
        {
            case State.Newspaper:
                this.GetComponent<Renderer>().material = paperMaterial;
                rigidBody.mass = 2;
                canPush = false;
                break;
            case State.Wood:
                this.GetComponent<Renderer>().material = woodMaterial;
                rigidBody.mass = 4;
                canPush = true;
                break;
            case State.Cement:
                this.GetComponent<Renderer>().material = cementMaterial;
                rigidBody.mass = 8f;
                canPush = true;
                break;
        }
    }

    public void StopMovement()
    {
        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;
    }

    public bool Push()
    {
        return canPush;
    }

    public IEnumerator InTransitionPlate(float stopTime, State newState)
    {
        canMove = false;
        ChangeState(newState);
        yield return new WaitForSeconds(stopTime);
        canMove = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        //if the player enters the wind Area
        if (other.gameObject.CompareTag("WindArea"))
        {
            WindArea = other.gameObject;
            inWindArea = true;
        }
           
    }

    private void OnTriggerExit(Collider other)
    {
        //if the player enters the wind Area
        if (other.gameObject.CompareTag("WindArea"))
        {
            WindArea = null;
            inWindArea = false;
        }
    }

    private void RayCasting()
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position, Vector3.down, out hit, maxRayDistance,layerMask))
        {
            Debug.DrawRay(transform.position, Vector3.down*maxRayDistance, Color.green);
            isGrounded = true;
        }
        else
        {
            Debug.DrawRay(transform.position, Vector3.down*maxRayDistance, Color.red);
            isGrounded = false;
        }
    }

    public bool GetGroundedStatus()
    {
        return isGrounded;
    }

    public void SetGroundedStatus(bool value)
    {
        isGrounded = value;
    }

    public void SetMass(float value)
    {
        rigidBody.mass = value;
    }
}
