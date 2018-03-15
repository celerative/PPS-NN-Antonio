using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstaculo : MonoBehaviour {

    [SerializeField] string LayerHitName = "CarCollider"; // The name of the layer set on each car

    private void OnCollisionEnter(Collision collision) // Once anything hits the wall
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer(LayerHitName)) // Make sure it's a car
        {
            collision.transform.GetComponent<Car>().WallHit(); // If it is a car, tell it that it just hit a wall
        }
    }
}
