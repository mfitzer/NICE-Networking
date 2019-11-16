using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NICE_Networking
{
    public class NetworkTransform : NetworkBehaviour
    {
        [Tooltip("Number of times per second the transform data will be refreshed.")]
        public int refreshRate = 30;

        /// <summary>
        /// [SyncTransform] Sync Transform data across the network { position, rotation, scale }
        /// [SyncRigidbody] In addition to syncing Transform data, also sync Rigidbody data across the network { velocity, angular velocity }
        /// </summary>
        private enum SyncMode { SyncTransform, SyncRigidbody }

        [SerializeField, Tooltip("How the transform will be synced over the network.")]
        private SyncMode syncMode = SyncMode.SyncTransform;

        /// <summary>
        /// [World] World transform values will be used, except for scale, which is always local.
        /// [Local] Local transform values will be used.
        /// </summary>
        public enum TransformSpace { World, Local }

        [SerializeField, Tooltip("What transform values will be synced over the network, world or local.")]
        private TransformSpace transformSpace = TransformSpace.World;

        [Header("Position")]

        [Tooltip("How far the transform must move before the network is updated.")]
        public float movementThreshold = 0.001f;

        [Tooltip("How far the transform can move before it's snapped to its network position.")]
        public float snapThreshold = 0.5f;

        [SerializeField, Range(0, 1), Tooltip("How fast the transform will interpolate to its network position. If set to 0 or 1, it will snap to the network position.")]
        private float interpolationMovementFactor = 0.5f;

        [Header("Rotation")]

        [SerializeField, Range(0, 1), Tooltip("How fast the transform will interpolate to its network rotation. If set to 0 or 1, it will snap to the network rotation.")]
        private float interpolationRotationFactor = 0.5f;

        /// <summary>
        /// Minimum number of times per second transform data will be sent over the network.
        /// </summary>
        public const int minRefreshRate = 1;

        #region Transform Data

        /// <summary>
        /// Position of the transform on the network.
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;

                //Not already handling a position update and don't have network authority
                if (positionUpdateHandler == null && !hasNetworkAuthority)
                {
                    if (enabled)
                        positionUpdateHandler = StartCoroutine(handlePositionUpdate());
                    else //Start the position update handler on enable
                        startPosUpdateHandler = true;
                }
            }
        }
        private Vector3 position = Vector3.zero;
        private Coroutine positionUpdateHandler;
        private bool startPosUpdateHandler = false;

        /// <summary>
        /// Current position of the transform in the set TransformSpace.
        /// </summary>
        private Vector3 currentPosition
        {
            get
            {
                return transformSpace == TransformSpace.World ? transform.position : transform.localPosition;
            }
        }

        /// <summary>
        /// Rotation of the transform on the network.
        /// </summary>
        public Quaternion Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                rotation = value;

                //Not already handling a rotation update and don't have network authority
                if (rotationUpdateHandler == null && !hasNetworkAuthority)
                {
                    if (enabled)
                        rotationUpdateHandler = StartCoroutine(handleRotationUpdate());
                    else //Start the rotation update handler on enable
                        startRotUpdateHandler = true;
                }
            }
        }
        private Quaternion rotation = Quaternion.identity;
        private Coroutine rotationUpdateHandler;
        private bool startRotUpdateHandler = false;

        /// <summary>
        /// Current rotation of the transform in the set TransformSpace in euler angles.
        /// </summary>
        private Quaternion currentRotation
        {
            get
            {
                return transformSpace == TransformSpace.World ? transform.rotation : transform.localRotation;
            }
        }

        /// <summary>
        /// Current euler angles of the transform in the set TransformSpace in euler angles.
        /// </summary>
        private Vector3 currentEulerAngles
        {
            get
            {
                return transformSpace == TransformSpace.World ? transform.eulerAngles : transform.localEulerAngles;
            }
        }

        /// <summary>
        /// Scale of the transform on the network.
        /// </summary>
        public Vector3 Scale
        {
            get
            {
                return scale;
            }
            set
            {
                scale = value;
                handleScaleUpdate();
            }
        }
        private Vector3 scale = Vector3.zero;

        /// <summary>
        /// Current scale of the transform in the set TransformSpace.
        /// </summary>
        private Vector3 currentScale
        {
            get
            {
                //Scale is always local scale
                return transform.localScale;
            }
        }

        //Transform data from the last update
        private Vector3 previousPosition = Vector3.zero;
        private Vector3 previousRotation = Vector3.zero;
        private Vector3 previousScale = Vector3.one;

        #endregion Transform Data

        #region Rigidbody Data

        /// <summary>
        /// Rigidbody attached to the NetworkTransform.
        /// </summary>
        public Rigidbody rb { get; private set; }

        /// <summary>
        /// Velocity of the rigidbody on the network.
        /// </summary>
        public Vector3 Velocity
        {
            get
            {
                return velocity;
            }
            set
            {
                velocity = value;
                handleVelocityUpdate();
            }
        }
        private Vector3 velocity = Vector3.zero;

        /// <summary>
        /// Angular velocity of the rigidbody on the network.
        /// </summary>
        public Vector3 AngularVelocity
        {
            get
            {
                return angularVelocity;
            }
            set
            {
                angularVelocity = value;
                handleAngularVelocityUpdate();
            }
        }
        private Vector3 angularVelocity = Vector3.zero;

        //Rigidbody data from the last update
        private Vector3 previousVelocity = Vector3.zero;
        private Vector3 previousAngularVelocity = Vector3.zero;

        #endregion Rigidbody Data        

        private void OnValidate()
        {
            if (refreshRate < minRefreshRate) //Ensure refresh rate does not go below min refresh rate
            {
                refreshRate = minRefreshRate;
            }
        }

        private void Start()
        {
            if (transformSpace == TransformSpace.World)
            {
                Position = transform.position;
                Rotation = transform.rotation;
                Scale = transform.localScale;
            }
            else //TransformSpace.Local
            {
                Position = transform.localPosition;
                Rotation = transform.localRotation;
                Scale = transform.localScale;
            }

            if (syncMode == SyncMode.SyncRigidbody)
            {
                rb = GetComponent<Rigidbody>();
                if (rb)
                {
                    velocity = rb.velocity;
                    angularVelocity = rb.angularVelocity;
                }
                else
                    Debug.LogError(name + "'s NetworkTransform does not have a Rigidbody");
            }
        }

        #region Network Sharing

        /// <summary>
        /// Refreshes transform data every (1 / refreshRate) seconds if the network transform has network authority
        /// </summary>
        private IEnumerator runNetworkRefresher()
        {
            while (true)
            {
                if (hasNetworkAuthority)
                {
                    refreshTransformData();
                    yield return new WaitForSeconds(1 / refreshRate);
                }
                else //Wait until next frame
                    yield return null;
            }
        }

        //Check if transform data has changed since the last refresh
        private void refreshTransformData()
        {
            bool positionChanged = false;
            Vector3 pos = currentPosition;
            if (Vector3.Distance(pos, previousPosition) >= movementThreshold) //Position moved past the movement threshold
            {
                positionChanged = true;
                previousPosition = pos;
            }

            bool rotationChanged = false;
            Vector3 rot = currentEulerAngles;
            if (previousRotation != rot)
            {
                rotationChanged = true;
                previousRotation = rot;
            }

            bool scaleChanged = false;
            Vector3 scale = currentScale;
            if (previousScale != scale)
            {
                scaleChanged = true;
                previousScale = scale;
            }

            #region Rigidbody

            bool velocityChanged = false;
            bool angularVelocityChanged = false;
            if (syncMode == SyncMode.SyncRigidbody && rb)
            {
                if (previousVelocity != rb.velocity)
                {
                    velocityChanged = true;
                    previousVelocity = rb.velocity;
                }

                if (previousAngularVelocity != rb.angularVelocity)
                {
                    angularVelocityChanged = true;
                    previousAngularVelocity = rb.angularVelocity;
                }
            }

            #endregion Rigidbody

            //Update the network if the position, rotation, or scale has changed
            if (positionChanged || rotationChanged || scaleChanged || velocityChanged || angularVelocityChanged)
                updateNetwork(positionChanged, rotationChanged, scaleChanged, velocityChanged, angularVelocityChanged);
        }

        //Updates the network with the new transform data
        private void updateNetwork(bool sendPosition, bool sendRotation, bool sendScale, bool sendVelocity, bool sendAngularVelocity)
        {
            if (hasNetworkAuthority)
            {
                bool clientMessage = networkIdentity.networkAuthority == NetworkAuthority.CLIENT; //Determines if the message should include the client ID

                //Sync transform data
                networkIdentity.sendMessage(MessageFactory.createTransformMessage(networkIdentity, transformSpace, sendPosition, sendRotation, sendScale, clientMessage));

                if (syncMode == SyncMode.SyncRigidbody)
                {
                    networkIdentity.sendMessage(MessageFactory.createRigidbodyMessage(networkIdentity, rb, sendVelocity, sendAngularVelocity, clientMessage));
                }
            }
        }

        #endregion Network Sharing

        #region Event Handling

        #region Transform

        private IEnumerator handlePositionUpdate()
        {
            //Only update position if this doesn't have network authority (following the network position)
            while (!hasNetworkAuthority && Position != currentPosition)
            {
                Vector3 newPosition;
                Vector3 currentPos = currentPosition;

                if (Vector3.Distance(Position, currentPos) >= snapThreshold || interpolationMovementFactor == 0) //Snap position
                    newPosition = Position;
                else //Interpolate between local position and network position
                    newPosition = Vector3.Lerp(currentPos, Position, interpolationMovementFactor);

                if (syncMode == SyncMode.SyncTransform)
                {
                    if (transformSpace == TransformSpace.World)
                        transform.position = newPosition;
                    else //TransformSpace.Local
                        transform.localPosition = newPosition;
                }
                else if (syncMode == SyncMode.SyncRigidbody)
                    rb.MovePosition(newPosition);

                yield return null;
            }

            positionUpdateHandler = null;
        }

        private IEnumerator handleRotationUpdate()
        {
            //Only update rotation if this doesn't have network authority (following the rotation position)
            while (!hasNetworkAuthority && Rotation != transform.rotation)
            {
                Quaternion newRotation;
                Quaternion currentRot = currentRotation;

                if (interpolationRotationFactor == 0) //Snap rotation
                    newRotation = Rotation;
                else //Interpolate between local rotation and network rotation
                    newRotation = Quaternion.Lerp(currentRotation, Rotation, interpolationRotationFactor);

                if (syncMode == SyncMode.SyncTransform)
                {
                    if (transformSpace == TransformSpace.World)
                        transform.rotation = newRotation;
                    else //TransformSpace.Local
                        transform.localRotation = newRotation;
                }
                else if (syncMode == SyncMode.SyncRigidbody)
                    rb.MoveRotation(newRotation);

                yield return null;
            }

            rotationUpdateHandler = null;
        }

        private void handleScaleUpdate()
        {
            if (Scale != transform.localScale)
            {
                transform.localScale = Scale;
            }
        }

        #endregion Transform

        #region Rigidbody

        private void handleVelocityUpdate()
        {
            if (rb && Velocity != rb.velocity)
            {
                rb.velocity = Velocity; //Update rigidbody velocity
            }
        }

        private void handleAngularVelocityUpdate()
        {
            if (rb && AngularVelocity != rb.angularVelocity)
            {
                rb.angularVelocity = AngularVelocity; //Update rigidbody angular velocity
            }
        }

        #endregion Rigidbody

        private void OnDisable()
        {
            //Ensures the position update handler will restart OnEnable
            if (positionUpdateHandler != null)
            {
                positionUpdateHandler = null;
                startPosUpdateHandler = true;
            }

            //Ensures the rotation update handler will restart OnEnable
            if (rotationUpdateHandler != null)
            {
                rotationUpdateHandler = null;
                startRotUpdateHandler = true;
            }
        }

        private void OnEnable()
        {
            //This has to restart the network refresher coroutine if the game object was disabled.
            StartCoroutine(runNetworkRefresher());

            //There is a position update that has not been handled
            if (startPosUpdateHandler)
            {
                positionUpdateHandler = StartCoroutine(handlePositionUpdate());
                startPosUpdateHandler = false;
            }

            //There is a rotation update that has not been handled
            if (startRotUpdateHandler)
            {
                rotationUpdateHandler = StartCoroutine(handleRotationUpdate());
                startRotUpdateHandler = false;
            }
        }

        #endregion Event Handling
    }
}
