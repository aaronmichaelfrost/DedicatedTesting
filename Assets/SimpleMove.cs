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


    private Mirror.Experimental.NetworkLerpRigidbody mirrorRb;


    public float correctionLerpLocal = .5f;


    [Mirror.SyncVar()]
    private Vector3 targetPosition;




    private void Start()
    {
        rb = GetComponent<Rigidbody>();


        // Start corrections thread on the server
        if (Mirror.NetworkServer.active) 
            StartCoroutine(MakeCorrections());
        else
        {
            mirrorRb = GetComponent<Mirror.Experimental.NetworkLerpRigidbody>();


            rb.isKinematic = true;
        }
            

        

    }


    private void OnDisable()
    {
        StopAllCoroutines();
    }



    void Update()
    {

        if (isLocalPlayer)
        {
            Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

            MoveServer(input);

            if(!Mirror.NetworkServer.active)
                MoveLocal(input);
        }

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


            CorrectPlayer(transform.position);
        }
    }



    /// <summary>
    /// Update the player's position and velocity to the server's 
    /// </summary>
    /// <param name="position"></param>
    /// <param name="velocity"></param>
    [Mirror.ClientRpc]
    private void CorrectPlayer(Vector3 position)
    {

        if(rb != null)
        {
            //rb.position = position;
            //rb.velocity = velocity;


            targetPosition = position;
        }

    }



    [Mirror.Command]
    private void MoveServer(Vector3 input)
    {
        // TODO Validate / normalize input first


        rb.velocity = input * speed;
    }

    Vector3 pos;

    private void MoveLocal(Vector3 input)
    {
        // Lerp pos to target position
        pos = Vector3.Lerp(transform.position, targetPosition, correctionLerpLocal);



        transform.position += input * speed * Time.fixedDeltaTime;


        // Lerp position to pos
        transform.position = Vector3.Lerp(transform.position, pos, correctionLerpLocal);
    }

}
