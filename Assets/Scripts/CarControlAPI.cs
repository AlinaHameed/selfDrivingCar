using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarControlAPI : MonoBehaviour
{
    // Start is called before the first frame update

    #region Variables
    PrometeoCarController controlScript;

    private float rightPower;
    private float leftPower;
    private float throttlePower;

    public bool planned, sensorLogic;
    private bool execute = true;


    #endregion

    #region Built In
    void Awake()
    {
        controlScript = GetComponent<PrometeoCarController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (planned)
        {
            PlannedMovement();
        }
        else if (sensorLogic)
        {
            SensorLogicMovement();
        }
    }

    #endregion

    #region Movement Controls
    // ---------- PRE-PLANNED MOVEMENT ------------

    void PlannedMovement()
    {
        //Always move forward at full throttle. SetThrottle takes in a float between -1 and 1, with 1 being full forward and -1 being full backward.
        controlScript.SetThrottle(1);

        if (execute)
        {
            double[] instructions = {0,0,-0.3,1.2,0,-0.4, 0,0.9,-1,0,0.3,0.2,0.3,0,0,0,0};

            //Function which will run those instructions one at a time and switch every 0.5 seconds. You can edit the time interval by changing the number on the next line.
            ExectuteInstructions(instructions, 0.5);
            //Only start this function once;
            execute = false;
        }

        void ExectuteInstructions(double[] instructions, double delay)
        {
            StartCoroutine(ExecuteInstructionsCoroutine());

            IEnumerator ExecuteInstructionsCoroutine()
            {
                int instructionIndex = 0;
                while (instructionIndex < instructions.Length)
                {
                    float currDuration = 0;

                    while (currDuration < delay)
                    {
                        currDuration += Time.deltaTime;
                        controlScript.SetTurn((float)instructions[instructionIndex]);
                        yield return new WaitForSeconds(0.001f);
                    }
                    instructionIndex++;
                }
            }
        }
    }
    //Takes in an array of instructions and exectutes them one at a time as movement commands (-1 is full left, 1 is full right, 0 is straight), waiting for delay seconds before moving to the next instruction



    // ------------------ SENSOR LOGIC ---------------

    void SensorLogicMovement()
    {

        //Always move forward at full throttle
        controlScript.SetThrottle(1);


        //See Raycast description in Utilities below
        float rightDist = Raycast(45);
        float leftDist = Raycast(-45);
        float frontDist = Raycast(0);

     
        if(frontDist >= 9.5f && leftDist >= 9.5f && rightDist >= 9.5f) {
            controlScript.SetThrottle(0.9f);
            controlScript.SetTurn(0);
        } else if (frontDist >= 9.5f && leftDist < 9.5f && rightDist >= 9.5f){
            controlScript.SetThrottle(0.9f);
            controlScript.SetTurn(1f);
        } else if (frontDist >= 9.5f && leftDist >= 9.5f && rightDist < 9.5f) {
            controlScript.SetThrottle(0.9f);
            controlScript.SetTurn(-1f);
        } else if (frontDist < 10f && leftDist < 9.5f){
            controlScript.SetThrottle(-0.9f);
            controlScript.SetTurn(-1f);
        } else if (frontDist < 10f && leftDist >= 9.5f){
            controlScript.SetThrottle(-0.9f);
            controlScript.SetTurn(1f);
        } else if (frontDist < 10f && leftDist < 9.5f && rightDist < 9.5f){
            controlScript.SetThrottle(-0.9f);
            controlScript.SetTurn(0f);
        } else {
            controlScript.SetThrottle(0.9f);
            controlScript.SetTurn(0f);
        }
    }
    #endregion

    #region Utilities

    //Takes in an angle as degrees, where 0 is the front of the car, and 180,-180 are the back.
    //Returns a float (number with decimals) representing the distance to the nearest object, or 1000 if it doesn't hit anything.
    float Raycast(float yAngleOffset)
    {


        var direction = Quaternion.Euler(0, yAngleOffset, 0) * transform.forward;
        var position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);

        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(position, direction, out hit, Mathf.Infinity))
        {
            Debug.DrawRay(position, direction * hit.distance, Color.yellow);
            return hit.distance;
        }
        else
        {
            Debug.DrawRay(position, direction * 50, Color.red);
            return 1000f;
        }
    }

    #endregion
}
