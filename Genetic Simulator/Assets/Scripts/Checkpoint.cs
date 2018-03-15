using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] string LayerHitName = "CarCollider"; // The name of the layer set on each car

    List<string> AllGuids = new List<string>(); // The list of Guids of all the cars increased

    private void OnTriggerEnter(Collider other) // Once anything goes through the wall
    {
        if(other.gameObject.layer == LayerMask.NameToLayer(LayerHitName)) // If this object is a car
        {
            Car CarComponent = other.transform.parent.GetComponent<Car>(); // Get the compoent of the car
            string CarGuid = CarComponent.TheGuid; // Get the Unique ID of the car

            if (!AllGuids.Contains(CarGuid)) // If we didn't increase the car before
            {
                AllGuids.Add(CarGuid); // Make sure we don't increase it again
                CarComponent.CheckpointHit(); // Increase the car's fitness
            }
        }
    }
}
