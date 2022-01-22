using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basically.Entities;
using Basically.Utility;
using Basically.Client;

public class PlayerShellEntity : PredictPhys3DEntity {
    public Transform orientation;
    public bool grounded;
    public new Camera camera;

    protected CapsuleCollider col;
    protected Vector3 floorNormal;

    protected Inputs[] inputBuffer = new Inputs[Basically.Utility.BGlobals.TICK_RATE];

    // correction
    private Vector3 oldPosition;
    private Vector3 oldVelocity;

    public override Quaternion Rotation => orientation.rotation;

    public Inputs CurrentInput {
        get {
            return inputBuffer[Basically.Utility.BGlobals.Tick % inputBuffer.Length];
        }

        set {
            inputBuffer[Basically.Utility.BGlobals.Tick % inputBuffer.Length] = value;
        }
    }

    #region Player Stuff

    protected override void Predict() {
        var input = CurrentInput;
        UpdateCamera(input);
        HandleMovement(input);
    }

    public Inputs SampleInputs() {
        var previous = inputBuffer[(Basically.Utility.BGlobals.Tick - 1) % inputBuffer.Length];

        var input = new Inputs {
            up = Input.GetKey(KeyCode.W),
            down = Input.GetKey(KeyCode.S),
            left = Input.GetKey(KeyCode.A),
            right = Input.GetKey(KeyCode.D),
            sprint = Input.GetKey(KeyCode.LeftShift),
            jump = Input.GetKey(KeyCode.Space),

            orientation = previous.orientation + (Input.GetAxis("Mouse X") * 5),
            viewAngle = Mathf.Clamp(previous.viewAngle - (Input.GetAxis("Mouse Y") * 5), -90f, 90f)
        };
        return input;
    }

    public void HandleMovement(Inputs inputs) {
        Rigidbody.AddForce(Vector3.down * Time.deltaTime * 10);

        // calculate direction
        var delta = Vector3.zero;
        var target = inputs.MovementVector * GetCurrentSpeed(inputs);

        // delta calculation
        if (inputs.MovementVector.x != 0 || grounded) delta.x = (target.x - RelativeVelocity.x) * Acceleration;
        if (inputs.MovementVector.y != 0 || grounded) delta.z = (target.y - RelativeVelocity.z) * Acceleration;

        Rigidbody.AddForce(orientation.TransformDirection(delta), ForceMode.Acceleration);

        // Jump
        if (inputs.jump && grounded) {
            Rigidbody.AddForce(Vector3.up * PlayerSettings.JumpSpeed * 0.50f, ForceMode.VelocityChange);
            Rigidbody.AddForce(floorNormal.normalized * PlayerSettings.JumpSpeed * 0.50f, ForceMode.VelocityChange);
            grounded = false;
        }
        Debug.DrawRay(transform.position, Rigidbody.velocity, Color.blue, Time.deltaTime);
    }

    public void UpdateCamera(Inputs inputs) {
        orientation.localEulerAngles = new Vector3(0, inputs.orientation);
        visualTransform.localEulerAngles = new Vector3(0, inputs.orientation);
        camera.transform.localEulerAngles = new Vector3(inputs.viewAngle, 0);

        if (Physics.Raycast(camera.transform.position, camera.transform.forward, out RaycastHit hit)) {
            Debug.DrawLine(camera.transform.position, hit.point, Color.red, Time.deltaTime);
        }
    }

    #region Ground Check

    // Thanks Dani.

    protected bool IsFloor(Vector3 v, float slope) {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < slope;
    }

    bool cancellingGrounded;

    private void OnCollisionStay(Collision other) {
        int layer = other.gameObject.layer;
        if (PlayerSettings.Ground != (PlayerSettings.Ground | (1 << layer))) return;

        for (int i = 0; i < other.contactCount; i++) {
            Vector3 normal = other.GetContact(i).normal;
            if (IsFloor(normal, PlayerSettings.SlopeLimit)) {
                floorNormal = normal;
                Rigidbody.useGravity = false;
                grounded = true;

                cancellingGrounded = false;
                CancelInvoke(nameof(StopGrounded));
            }
        }

        float delay = 3f;
        if (!cancellingGrounded) {
            cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }

    private void StopGrounded() {
        grounded = false;
        Rigidbody.useGravity = true;
    }

    #endregion

    #region Utilities

    // all of these are relative to view
    // z: how much you are moving forward
    // x: how much you are moving to the right
    public Vector3 RelativeVelocity { get { return orientation.InverseTransformDirection(Rigidbody.velocity); } }

    public float Acceleration { get { return grounded ? PlayerSettings.AccelerateSpeed : PlayerSettings.AirAccelerate; } }

    public float GetCurrentSpeed(Inputs input) {
        return input.sprint ? PlayerSettings.SprintSpeed : PlayerSettings.RunningSpeed;
    }

    #endregion

    #endregion

    #region Entity Callbacks
    #if BASICALLY_CLIENT

    protected override void OnClientTick() {
        var input = SampleInputs();
        CurrentInput = input;
        NetworkClient.Send(input, 0, Basically.Networking.MessageType.Unreliable);

        base.OnClientTick();
    }

    #endif

    protected override void UpdateError() {
        visualTransform.transform.position = transform.position + PositionError;
    }

    protected override void InterpFunc(EntityState from, EntityState to, float interpAmount) {
        // velocity
        Vector3 fromVel = from.parameters.Get<Vector3>("vel") * Time.deltaTime;
        Vector3 toVel = to.parameters.Get<Vector3>("vel") * Time.deltaTime;

        // view angles
        float fromAngle = from.parameters.Get<ushort>("view").UnbindToFloat(-90f, 90f);
        float toAngle = to.parameters.Get<ushort>("view").UnbindToFloat(-90f, 90f);

        transform.position = Curves.HermiteUnclamped(from.position, fromVel, to.position, toVel, interpAmount);
        orientation.rotation = Quaternion.SlerpUnclamped(from.rotation, to.rotation, interpAmount);
        camera.transform.localEulerAngles = new Vector3(Mathf.Lerp(fromAngle, toAngle, interpAmount), 0);
    }

    protected override void Serialize(ref IParameters param) {
        base.Serialize(ref param);

        // Binding here reduces the amount of bytes used per snapshot, while maintaining accurate view angles on the server
        param.Add("view", camera.transform.localEulerAngles.x.BindToUShort(-90f, 90f));
    }

    protected override uint NeedsCorrection(EntityState client, EntityState server) {
        var posErr = server.position - client.position;

        if (posErr.sqrMagnitude > positionErrorThreshold) {
            oldPosition = Rigidbody.position;
            oldVelocity = Rigidbody.velocity;
            prevPosCache = Rigidbody.position + PositionError;

            Rigidbody.position = server.position;
            Rigidbody.velocity = server.parameters.Get<Vector3>("vel");

            return Basically.Utility.BGlobals.Tick - server.Tick;
        }
        return 0;
    }

    protected override void CorrectFinalize() {
        if ((prevPosCache - Rigidbody.position).sqrMagnitude >= 4.0f) {
            PositionError = Vector3.zero;
        } else {
            PositionError = prevPosCache - Rigidbody.position;
        }
    }

    #endregion
}
