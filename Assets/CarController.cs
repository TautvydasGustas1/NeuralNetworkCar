using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NNet))]

public class CarController : MonoBehaviour
{
    private Vector3 startPosition, startRotation;
    private NNet network;

    [Range(-1f, 1f)]
    public float a,t;

    private Vector3 inp;


    //For checking if its too idle
    public float timeSinceStart = 0f;

    //Fitness checking how fast and how far car goes
    [Header("Fitness")]
    public float overallFitness;

    //For how important variable is
    public float distanceMultipler = 1.4f;
    public float avgSpeedMultiplier = 0.2f;
    public float sensorMultiplier = 0.1f;


    [Header("Network Options")]
    public int LAYERS = 1;
    public int NEURONS = 10;


    //Variables for calculating the fitness
    private Vector3 lastPosition;
    private float totalDistanceTravelled;
    private float avgSpeed;


    public float aSensor, bSensor, cSensor;

    private void Awake() {
        startPosition = transform.position;
        startRotation = transform.eulerAngles;
        network = GetComponent<NNet>();

        //Test CODE
        network.Initialise(LAYERS, NEURONS);
    }


    public void Reset() {

        //Test CODE
        network.Initialise(LAYERS, NEURONS);

        timeSinceStart = 0f;
        totalDistanceTravelled = 0f;
        avgSpeed = 0f;
        lastPosition = startPosition;
        overallFitness = 0f;
        transform.position = startPosition;
        transform.eulerAngles = startRotation;
        
    }


    private void OnCollisionEnter(Collision collision) {
        Reset();
    }

    private void CalculateFitness() {
        totalDistanceTravelled += Vector3.Distance(transform.position, lastPosition);
        avgSpeed = totalDistanceTravelled/timeSinceStart;

        overallFitness = (totalDistanceTravelled * distanceMultipler) + (avgSpeed * avgSpeedMultiplier) + (((aSensor + bSensor+ cSensor)/ 3)*sensorMultiplier);

        if(timeSinceStart > 20 && overallFitness < 40) {
            Reset();
        }

        if(overallFitness >= 1000) {
            //Saves network to a JSON
            Reset();
        }

       // a=0f;
       // t=0f;
    }

    private void FixedUpdate() {
        InputSensors();
        lastPosition = transform.position;

        (a, t) = network.RunNetwork(aSensor, bSensor, cSensor);
        
        MoveCar(a, t);

        timeSinceStart += Time.deltaTime;

        CalculateFitness();
        
    }

    private void InputSensors() {
        Vector3 a = (transform.forward + transform.right);
        Vector3 b = transform.forward;
        Vector3 c = transform.forward - transform.right;
        
        Ray r = new Ray(transform.position, a);
        RaycastHit hit;


        //Normalization values needs to be 0 or 1
        if(Physics.Raycast(r, out hit)) {
            aSensor = hit.distance/20;
            Debug.DrawLine(r.origin, hit.point, Color.red);
        }

        r.direction = b;

         if(Physics.Raycast(r, out hit)) {
            bSensor = hit.distance/30;
            Debug.DrawLine(r.origin, hit.point, Color.red);
        }

         r.direction = c;

         if(Physics.Raycast(r, out hit)) {
            cSensor = hit.distance/20;
            Debug.DrawLine(r.origin, hit.point, Color.red);
        }
    }

    public void MoveCar(float v, float h) {
        inp = Vector3.Lerp(Vector3.zero, new Vector3(0, 0, v*11.4f), 0.02f);
        inp = transform.TransformDirection(inp);
        transform.position += inp;

        transform.eulerAngles += new Vector3(0, h*90*0.02f, 0);
    }
}
