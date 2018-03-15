using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Car : MonoBehaviour
{
    [SerializeField] bool UseUserInput = false; // Defines whether the car uses a NeuralNetwork or user input
    [SerializeField] LayerMask SensorMask; // Defines the layer of the walls ("Wall")

    public static NeuralNetwork NextNetwork = new NeuralNetwork(new uint[] { 6, 32, 32, 32, 32, 2 }, null); // public NeuralNetwork that refers to the next neural network to be set to the next instantiated car

    public string TheGuid { get; private set; } // The Unique ID of the current car

    public double Fitness { get; private set; } // The fitness/score of the current car. Represents the number of checkpoints that his car hit.
    public double sigma = 1e-07;
    public NeuralNetwork TheNetwork { get; private set; } // The NeuralNetwork of the current car

    Rigidbody TheRigidbody; // The Rigidbody of the current car
    LineRenderer TheLineRenderer; // The LineRenderer of the current car
    public double starttime;
    private void Awake()
    {
        TheGuid = Guid.NewGuid().ToString(); // Assigns a new Unique ID for the current car

        TheNetwork = NextNetwork; // Sets the current network to the Next Network
        NextNetwork = new NeuralNetwork(NextNetwork.Topology, null); // Make sure the Next Network is reassigned to avoid having another car use the same network

        TheRigidbody = GetComponent<Rigidbody>(); // Assign Rigidbody
        TheLineRenderer = GetComponent<LineRenderer>(); // Assign LineRenderer

        StartCoroutine(IsNotImproving()); // Start checking if the score stayed the same for a lot of time


        starttime = Time.time;
        TheLineRenderer.positionCount = 25; // Make sure the line is long enough
    }
    public float salida1, salida2;
    public float[] NeuralOutput;
    private void FixedUpdate()
    {
        if (UseUserInput) // If we're gonna use user input
            Move(Input.GetAxisRaw("Vertical"), Input.GetAxisRaw("Horizontal")); // Moves the car according to the input
        else // if we're gonna use a neural network
        {
            float Vertical;
            float Horizontal;

            GetNeuralInputAxis(out Vertical, out Horizontal);

            Move(salida1, salida2); // Moves the car
        }
        
    }

    // Casts all the rays, puts them through the NeuralNetwork and outputs the Move Axis

    void GetNeuralInputAxis(out float Vertical, out float Horizontal)
    {
        float[] NeuralInput = new float[NextNetwork.Topology[0]];

        // Cast forward, back, right and left
        NeuralInput[0] = CastRay(transform.forward, Vector3.forward, 1) / RayLength;
        NeuralInput[1] = CastRay(-transform.forward, -Vector3.forward, 3) / RayLength;
        NeuralInput[2] = CastRay(transform.right, Vector3.right, 5) / RayLength;
        NeuralInput[3] = CastRay(-transform.right, -Vector3.right, 7) / RayLength;

        // Cast forward-right and forward-left
        float dir = 4*Mathf.PI / 10;
        NeuralInput[4] = CastRay(transform.right * Mathf.Sin(dir) + transform.forward * Mathf.Cos(dir), Vector3.right * Mathf.Sin(dir) + Vector3.forward * Mathf.Cos(dir), 9) / RayLength;
        NeuralInput[5] = CastRay(transform.right * Mathf.Sin(dir) + -transform.forward * Mathf.Cos(dir), Vector3.right * Mathf.Sin(dir) + -Vector3.forward * Mathf.Cos(dir), 13) / RayLength;

        // Feed through the network
        NeuralOutput = TheNetwork.FeedForward(NeuralInput);

        // Get Vertical Value
        if (NeuralOutput[0] <= 0.35f)
            Vertical = -1;
        else if (NeuralOutput[0] >= 0.65f)
            Vertical = 1;
        else
            Vertical = 0;

        // Get Horizontal Value
        if (NeuralOutput[1] <= 0.45f)
            Horizontal = -1;
        else if (NeuralOutput[1] >= 0.55f)
            Horizontal = 1;
        else
            Horizontal = 0;

        // If the output is just standing still, then move the car forward
        if (Vertical == 0 && Horizontal == 0)
            Vertical = 1;

        salida1 = Convert.ToSingle(NeuralOutput[0]);
        salida2 = Convert.ToSingle(NeuralOutput[1]);
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

        }
    }
    public float RayLength = 4;         // Maximum length of each ray

    // Casts a ray and makes it visible through the line renderer
    float CastRay(Vector3 RayDirection, Vector3 LineDirection, int LinePositionIndex)
    {

        RaycastHit Hit;
        if (Physics.Raycast(transform.position, RayDirection, out Hit, RayLength, SensorMask)) // Cast a ray
        {
            float Dist = Vector3.Distance(Hit.point, transform.position); // Get the distance of the hit in the line
            TheLineRenderer.SetPosition(LinePositionIndex, Dist * LineDirection); // Set the position of the line

            return Dist; // Return the distance
        }
        else
        {
            TheLineRenderer.SetPosition(LinePositionIndex, LineDirection * RayLength); // Set the distance of the hit in the line to the maximum distance

            return RayLength; // Return the maximum distance
        }
    }
    const float MAX_FORCE = 25;
    // The main function that moves the car.
    public void Move(float v, float h)
    {
        float a = MAX_FORCE * v;
        float b = MAX_FORCE * h;
        TheRigidbody.velocity = transform.right * a;
        TheRigidbody.angularVelocity = transform.up * b;
        //TheRigidbody.AddRelativeForce(transform.right * a);
        //TheRigidbody.AddRelativeTorque(transform.up * b);


    }

    public double delta = 0;
    public UInt16 cantCheckpoints = 0;
    // This function is called through all the checkpoints when the car hits any.
    public void CheckpointHit()
    {
        cantCheckpoints++;
        delta = Time.time - starttime;
        starttime = Time.time;
        Fitness = (1.1 * Fitness) + 1 / (1 + delta); // Increase Fitness/Score
    }

    // Called by walls when hit by the car
    public void WallHit()
    {
        EvolutionManager.Singleton.CarDead(this, Fitness); // Tell the Evolution Manager that the car is dead
        gameObject.SetActive(false); // Make sure the car is inactive
    }

    [SerializeField] string LayerHitName1 = "CarCollider"; // The name of the layer set on each car
    [SerializeField] string LayerHitName2 = "Wall"; // The name of the layer set on each Wall
    
    
}
