using UnityEngine;


namespace Mirror.Experimental
{
    [AddComponentMenu("Network/Experimental/NetworkLerpRigidbody")]
    [HelpURL("https://mirror-networking.com/docs/Articles/Components/NetworkLerpRigidbody.html")]
    public class NetworkLerpRigidbody : NetworkBehaviour
    {
        [Header("Settings")]
        [SerializeField] internal Rigidbody rb = null;
        [Tooltip("How quickly current velocity approaches target velocity")]
        [SerializeField] public float lerpVelocityAmount = 0.5f;
        [Tooltip("How quickly current position approaches target position")]
        [SerializeField] public float lerpPositionAmount = 0.5f;

        [Tooltip("Set to true if moves come from owner client, set to false if moves always come from server")]
        [SerializeField] bool clientAuthority = false;

        float nextSyncTime;


        [SyncVar()]
        public Vector3 targetVelocity;

        [SyncVar()]
        public Vector3 targetPosition;

        /// <summary>
        /// Ignore value if is host or client with Authority
        /// </summary>
        /// <returns></returns>
        bool IgnoreSync => isServer || ClientWithAuthority;

        bool ClientWithAuthority => clientAuthority && hasAuthority;

        


        void OnValidate()
        {
            if (rb == null)
            {
                rb = GetComponent<Rigidbody>();
            }

        }

        void Update()
        {
            if (isServer)
            {
                SyncToClients();
            }
            else if (ClientWithAuthority)
            {
                SendToServer();
            }


            // Dont update the players position if they are the local player
            if (IgnoreSync) { return; }



            rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, lerpVelocityAmount);
            rb.position = Vector3.Lerp(rb.position, targetPosition, lerpPositionAmount);
            // add velocity to position as position would have moved on server at that velocity
            targetPosition += rb.velocity * Time.deltaTime;
        }

        private void SyncToClients()
        {
            targetVelocity = rb.velocity;
            targetPosition = rb.position;
        }

        

        private void SendToServer()
        {
            float now = Time.time;
            if (now > nextSyncTime)
            {
                nextSyncTime = now + syncInterval;
                CmdSendState(rb.velocity, rb.position);
            }
        }

        [Command]
        private void CmdSendState(Vector3 velocity, Vector3 position)
        {
            rb.velocity = velocity;
            rb.position = position;
            targetVelocity = velocity;
            targetPosition = position;
        }

        


    }
}
