using System.Collections;
using UnityEngine;

public class SimpleMove : Mirror.NetworkBehaviour
{


    private Rigidbody rb;

    public float speed;





    private void Start()
    {
        rb = GetComponent<Rigidbody>();


        // Start corrections thread on the server
        if (!isLocalPlayer)
            rb.isKinematic = true;

    }




    void Update()
    {

        if (isLocalPlayer)
        {
            Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

            Move(input);
        }

    }







    private void Move(Vector3 input)
    {
        rb.position += input * speed * Time.deltaTime;
    }

}
