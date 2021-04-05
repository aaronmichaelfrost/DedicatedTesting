using System.Collections;
using UnityEngine;

public class SimpleMove : Mirror.NetworkBehaviour
{


    private Rigidbody rb;

    public float speed;

    /// <summary>
    /// Movement corrections per second
    /// </summary>
    public float tickRate = 1;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();


        // Start corrections thread on the server
        if (Mirror.NetworkServer.active)
            StartCoroutine(MakeCorrections());

    }


    private void OnDisable()
    {
        StopAllCoroutines();
    }



    void Update()
    {
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        input *= speed;

        MoveServer(input);
        MoveLocal(input);
    }



    /// <summary>
    /// Sends the server's movement information to the player
    /// </summary>
    /// <returns></returns>
    private IEnumerator MakeCorrections()
    {
        for(; ; )
        {
            yield return new WaitForSeconds(Mathf.Pow(tickRate, -1));

            CorrectPlayer(transform.position, rb.velocity);
        }
    }



    /// <summary>
    /// Update the player's position and velocity to the server's 
    /// </summary>
    /// <param name="position"></param>
    /// <param name="velocity"></param>
    [Mirror.ClientRpc]
    private void CorrectPlayer(Vector3 position, Vector3 velocity)
    {
        transform.position = position;
        rb.velocity = velocity;
    }



    [Mirror.Command]
    private void MoveServer(Vector3 input)
    {
        rb.velocity = input;
    }



    private void MoveLocal(Vector3 input)
    {
        rb.velocity = input;
    }

}
