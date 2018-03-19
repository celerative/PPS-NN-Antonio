using System.Collections.Generic;
using UnityEngine;

public class LineaBuena : MonoBehaviour
{
    [SerializeField] string LayerHitName = "CarCollider"; // The name of the layer set on each car

    public List<string> AllGuids = new List<string>(); // The list of Guids of all the cars increased

    private void OnTriggerEnter(Collider other) // Once anything goes through the wall
    {
        if (other.gameObject.layer == LayerMask.NameToLayer(LayerHitName)) // If this object is a car
        {
            Car CarComponent = other.transform.parent.GetComponent<Car>(); // Get the compoent of the car
            string CarGuid = CarComponent.TheGuid; // Get the Unique ID of the car
            if (!AllGuids.Contains(CarGuid))
            {
                AllGuids.Add(CarGuid);
                CarComponent.PowerUpHit();

            }
        }
    }
    public void reset(string guid)
    {
        if (AllGuids.Contains(guid))
            AllGuids.Remove(guid);
    }
}