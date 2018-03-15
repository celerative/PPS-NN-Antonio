using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    Vector3 SmoothPosVelocity; // Velocity of Position Smoothing
    Vector3 SmoothRotVelocity; // Velocity of Rotation  Smoothing

    void FixedUpdate()
    {
        if (EvolutionManager.filmar)
        {
            Car BestCar = transform.GetChild(0).GetComponent<Car>(); // The best car in the bunch is the first one

            for (int i = 1; i < transform.childCount; i++) // Loop over all the cars
            {
                Car CurrentCar = transform.GetChild(i).GetComponent<Car>(); // Get the component of the current car

                if (CurrentCar.Fitness > BestCar.Fitness) // If the current car is better than the best car
                {
                    BestCar = CurrentCar; // Then, the best car is the current car
                }
            }

            Transform BestCarCamPos = BestCar.transform.GetChild(0); // The target position of the camera relative to the best car

            Camera.main.transform.position = Vector3.SmoothDamp(Camera.main.transform.position, BestCarCamPos.position, ref SmoothPosVelocity, 0.7f); // Smoothly set the position

            Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation,
                                                             Quaternion.LookRotation(BestCar.transform.position - Camera.main.transform.position),
                                                             0.1f); // Smoothly set the rotation
        }
    }
}