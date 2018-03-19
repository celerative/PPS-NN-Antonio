using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Car : MonoBehaviour
{
    [SerializeField] LayerMask SensorMask; // Defines the layer of the walls ("Wall")
    [SerializeField] LayerMask SensorMask2; // Defines the layer of the walls ("powerup")


    public static NeuralNetwork NextNetwork = new NeuralNetwork(new uint[] { 10, 32, 32, 16, 16, 3 }, null); // public NeuralNetwork that refers to the next neural network to be set to the next instantiated car

    public string TheGuid { get; private set; } // The Unique ID of the current car

    public double Fitness { get; private set; } // The fitness/score of the current car. Represents the number of checkpoints that his car hit.
    public NeuralNetwork TheNetwork { get; private set; } // The NeuralNetwork of the current car
    private int MyID;
    Rigidbody TheRigidbody; // The Rigidbody of the current car
    Collider TheCollider;
    LineRenderer TheLineRenderer; // The LineRenderer of the current car
    public float startTimeCheck;
    public float startTimePow;

    private void Awake()
    {
        TheGuid = Guid.NewGuid().ToString(); // Assigns a new Unique ID for the current car
        MyID = GetInstanceID();
        TheNetwork = NextNetwork; // Sets the current network to the Next Network
        NextNetwork = new NeuralNetwork(NextNetwork.Topology, null); // Make sure the Next Network is reassigned to avoid having another car use the same network

        TheRigidbody = GetComponent<Rigidbody>(); // Assign Rigidbody
        TheLineRenderer = GetComponent<LineRenderer>(); // Assign LineRenderer
        TheCollider = GetComponentInChildren<BoxCollider>();
        StartCoroutine(IsNotImproving()); // Start checking if the score stayed the same for a lot of time


        startTimeCheck = Time.time;
        startTimePow = startTimeCheck;
        TheLineRenderer.positionCount = 50; // Make sure the line is long enough
    }
    public float salida1, salida2, salida3, salida4;
    public float[] NeuralOutput;
    private void FixedUpdate()
    {

        GetNeuralInputAxis();
        Move(salida1, salida2, salida3); // Moves the car


    }

    // Casts all the rays, puts them through the NeuralNetwork and outputs the Move Axis
    public float[] NeuralInput;
    void GetNeuralInputAxis()
    {
        NeuralInput = new float[NextNetwork.Topology[0]];

        // Cast forward, back, right and left
        NeuralInput[0] = CastRay(transform.forward, Vector3.forward, 1) / RayLength;
        NeuralInput[1] = CastRay(-transform.forward, -Vector3.forward, 3) / RayLength;
        NeuralInput[2] = CastRay(transform.right, Vector3.right, 5) / RayLength;
        NeuralInput[3] = CastRay(-transform.right, -Vector3.right, 7) / RayLength;

        // Cast forward-right and forward-left
        float dir = 9 * Mathf.PI / 20;
        NeuralInput[4] = CastRay(transform.right * Mathf.Sin(dir) + transform.forward * Mathf.Cos(dir), Vector3.right * Mathf.Sin(dir) + Vector3.forward * Mathf.Cos(dir), 9) / RayLength;
        NeuralInput[5] = CastRay(transform.right * Mathf.Sin(dir) + -transform.forward * Mathf.Cos(dir), Vector3.right * Mathf.Sin(dir) + -Vector3.forward * Mathf.Cos(dir), 13) / RayLength;

        float dir2 = Mathf.PI / 4;
        NeuralInput[6] = CastRay(transform.right * Mathf.Sin(dir2) + transform.forward * Mathf.Cos(dir2), Vector3.right * Mathf.Sin(dir2) + Vector3.forward * Mathf.Cos(dir2), 15) / RayLength;
        NeuralInput[7] = CastRay(transform.right * Mathf.Sin(dir2) + -transform.forward * Mathf.Cos(dir2), Vector3.right * Mathf.Sin(dir2) + -Vector3.forward * Mathf.Cos(dir2), 17) / RayLength;



        // Cast forward-right and forward-left
        NeuralInput[8] = CastRay2(transform.right * Mathf.Sin(dir) + transform.forward * Mathf.Cos(dir), Vector3.right * Mathf.Sin(dir) + Vector3.forward * Mathf.Cos(dir), 19) / RayLength2;
        NeuralInput[9] = CastRay2(transform.right * Mathf.Sin(dir) + -transform.forward * Mathf.Cos(dir), Vector3.right * Mathf.Sin(dir) + -Vector3.forward * Mathf.Cos(dir), 21) / RayLength2;


        // Feed through the network
        NeuralOutput = TheNetwork.FeedForward(NeuralInput);

        // Get Vertical Value


        salida1 = Convert.ToSingle(NeuralOutput[0]);
        salida2 = Convert.ToSingle(NeuralOutput[1]);
        salida3 = Convert.ToSingle(NeuralOutput[2]);


    }

    // Checks each few seconds if the car didn't make any improvement
    IEnumerator IsNotImproving()
    {
        while (true)
        {
            double OldFitness = Fitness; // Save the initial fitness
            yield return new WaitForSeconds(2f); // Wait for some time
            if (OldFitness == Fitness) // Check if the fitness didn't change yet
                WallHit(); // Kill this car
            else if ((deltaCheck > 1f) || ((Time.time - startTimePow > 0.5f)))
                WallHit();
            else if (CantPowerUps == 0)
                WallHit();

        }
    }
    public float RayLength = 6;         // Maximum length of each ray
    public float RayLength2 = 0.1f;         // Maximum length of each ray


    // Casts a ray and makes it visible through the line renderer
    float CastRay(Vector3 RayDirection, Vector3 LineDirection, int LinePositionIndex)
    {

        RaycastHit Hit;
        if (Physics.Raycast(transform.position, RayDirection, out Hit, RayLength, SensorMask)) // Cast a ray
        {
            if (Hit.collider.GetInstanceID() != MyID)
            {
                float Dist = Vector3.Distance(Hit.point, transform.position); // Get the distance of the hit in the line
                TheLineRenderer.SetPosition(LinePositionIndex, Dist * LineDirection); // Set the position of the line

                return Dist; // Return the distance
            }
            else
            {
                TheLineRenderer.SetPosition(LinePositionIndex, LineDirection * RayLength); // Set the distance of the hit in the line to the maximum distance

                return RayLength; // Return the maximum distance }
            }
        }
        else
        {
            TheLineRenderer.SetPosition(LinePositionIndex, LineDirection * RayLength); // Set the distance of the hit in the line to the maximum distance

            return RayLength; // Return the maximum distance
        }
    }

    float CastRay2(Vector3 RayDirection, Vector3 LineDirection, int LinePositionIndex)
    {

        RaycastHit Hit;
        if (Physics.Raycast(transform.position, RayDirection, out Hit, RayLength2, SensorMask2)) // Cast a ray
        {

            float Dist = Vector3.Distance(Hit.point, transform.position); // Get the distance of the hit in the line
            float valor = Mathf.Exp(-Dist);
            TheLineRenderer.SetPosition(LinePositionIndex, Dist * LineDirection); // Set the position of the line
            return valor;
        }
        else
        {

            return 0; // Return the maximum distance
        }
    }





    const float MAX_FORCE = 30;
    const float MAX_TURN = 30;
    // The main function that moves the car.
    public void Move(float acelerar, float frenar, float izq)
    {

        TheCollider.material.dynamicFriction = frenar;
        float a = MAX_FORCE * acelerar;
        float b = a;
        izq *= MAX_TURN;
        TheRigidbody.angularVelocity = transform.up * izq;

        TheRigidbody.velocity = -transform.right * b;




    }

    public float deltaCheck = 0;
    public float deltaPowerUp = 0;

    public int cantCheckpoints = 0;
    public static int MaxCantPowerUps = -1;
    public int CantPowerUps = 0;

    // This function is called through all the checkpoints when the car hits any.
    public void CheckpointHit()
    {
        cantCheckpoints++;
        deltaCheck = Time.time - startTimeCheck;
        startTimeCheck = Time.time;
        Fitness = (1 + Fitness) + 5 / (1 + deltaCheck); // Increase Fitness/Score
    }

    public void PowerUpHit()
    {

        CantPowerUps++;

        if (CantPowerUps > MaxCantPowerUps)
        {
            MaxCantPowerUps = CantPowerUps;
        }
        deltaPowerUp = Time.time - startTimePow;
        startTimePow = Time.time;

        Fitness += 3 + 5 * Mathf.Exp(-deltaPowerUp);
    }


    private void OnCollisionEnter(Collision collision) // Once anything hits the wall
    {
        if (collision.gameObject.CompareTag("Car")) // Make sure it's a car
        {
            Debug.Log("Colisionó");
            WallHit(); // If it is a car, tell it that it just hit a wall
        }
    }



    // Called by walls when hit by the car
    public void WallHit()
    {
        EvolutionManager.Singleton.CarDead(this, Fitness); // Tell the Evolution Manager that the car is dead
        gameObject.SetActive(false); // Make sure the car is inactive
    }




}
