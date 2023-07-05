using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hermes.Cinemachine.Sample
{
    /// <summary>
    /// SphereMove
    /// </summary>
    public class SphereMove : MonoBehaviour
    {
        [SerializeField] Rigidbody rb;
        [SerializeField] float speed;

        Vector3 movement = Vector3.zero;

        // Update is called once per frame
        void Update()
        {
            movement.x = Input.GetAxis("Horizontal");
            movement.z = Input.GetAxis("Vertical");
            rb.AddForce(movement * speed);
        }
    }
}