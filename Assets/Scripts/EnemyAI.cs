using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public enum AIState
    {
        NonAlert,
        Alert,
        Engaged,
        Searching,
        NearNPC,
        Dead
    }
    private NavMeshAgent _agent;
    [SerializeField]
    private GameObject _target;
    // Start is called before the first frame update
    [SerializeField]
    private GameObject[] _targets;

    [SerializeField]
    private AIState _currentState;

    [SerializeField]
    private bool _standingGuard = true;

    [SerializeField]
    private List<GameObject> _wayPoints;
    private int _currentWayPoint;

    [SerializeField]
    private float _eyeDistance = 100.0f;

    // [SerializeField]
    // private bool _targetVisible = false;

    [SerializeField]
    private float _fieldOfView = 140.0f;

    [SerializeField]
    private float _alertThreshold = 2.0f;
    private float _alertTimeStamp = 1f;


    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();

        // TODO . choose one of the two implementation
        if (_target == null)
        {
            _target = GameObject.FindGameObjectWithTag("Player");
            if (_target == null)
            {
                Debug.LogError("Target failed to be assigned. Missing tag of Player");
            }
        }

        _targets = GameObject.FindGameObjectsWithTag("Player");
        Debug.Log("targets tot : " + _targets.Length);
        foreach (GameObject t in _targets)
        {
            Debug.Log(t.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Finite State Machine system
        switch (_currentState)
        {
            case AIState.NonAlert:
                // Debug.Log("Non alert");
                // if (eyeDetect(_target))
                if (Time.frameCount % 10 == 0)
                {
                    if (eyeDetectDetailed(_targets))
                    {
                        _currentState = AIState.Alert;
                        _alertTimeStamp = 0;
                    }

                    // check nearby Enemies
                    // every 10 frames, not all of them
                    if (Time.frameCount % 10 == 0)
                    {

                    }
                    // sphereCastNonAlloc
                    // all AI in sphere
                    // select the closest AI
                    // both the AI and me
                    // roll the dice
                    // do wanna both talk to each other ?
                    // go to nearNPC state
                }

                if (_standingGuard)
                {
                    // check enemy in radius
                    // if so, _currentState = AIState.Alert
                    return;
                }
                else
                {
                    // cycle through way points
                    if (_wayPoints.Count > 0)
                    {
                        _agent.SetDestination(_wayPoints[_currentWayPoint].transform.position);

                        float distanceToWayPoint = Vector3.Distance(
                                Vector3.ProjectOnPlane(transform.position, Vector3.up),
                                Vector3.ProjectOnPlane(_wayPoints[_currentWayPoint].transform.position, Vector3.up)
                            );

                        if (distanceToWayPoint < 1.0f)  // we arrive at the waypoint, plan next one
                        {
                            if (_currentWayPoint >= _wayPoints.Count - 1)
                            {
                                _currentWayPoint = 0;
                            }
                            else
                            {
                                _currentWayPoint++;
                            }
                        }
                    }

                }

                break;
            case AIState.Alert:
                // Debug.Log("Alert");

                _agent.SetDestination(transform.position);

                _alertTimeStamp += Time.deltaTime;
                if (_alertTimeStamp > _alertThreshold)
                {
                    if (eyeDetect(_target))
                    {
                        _currentState = AIState.Searching;
                    }
                }

                break;
            case AIState.Engaged:
                Debug.Log("Engaged");
                _agent.SetDestination(_target.transform.position);
                break;
            case AIState.Searching:
                Debug.Log("Searching");
                break;
            case AIState.NearNPC:

                break;
            default:
                break;
        }
    }


    private bool eyeDetectDetailed(GameObject[] others)
    {
        bool anyEyeDetectOther = false;
        foreach (GameObject other in others)
        {
            // TODO if keeping Detailed Detection, maybe split
            // eyeDetect to Raycast only if main main body is in range (extract 1st tests)
            if (eyeDetect(other))
            {
                anyEyeDetectOther = true;
                break;
            }
        }
        return anyEyeDetectOther;
    }

    private bool eyeDetect(GameObject other)
    {
        bool eyeDetectOther = false;
        float distanceToTarget = Vector3.Distance(transform.position, other.transform.position);
        // low cost test prior going further to target detection
        // see in the later if this test shalle be 1st or preferably _fieldOfView...
        if (distanceToTarget <= _eyeDistance)
        {
            Vector3 toOther = (other.transform.position - transform.position).normalized;
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            float angle = Vector3.Angle(forward, toOther);
            RaycastHit hit;
            if (angle <= _fieldOfView / 2 &&
                // TODO . RaycastNonAlloc instead ?!
                Physics.Raycast(transform.position, toOther, out hit, distanceToTarget)
                // Physics.RaycastNonAlloc(transform.position, toOther, out hit, distanceToTarget)
            )
            {
                // Debug.Log("Angle to target = " + angle + " / " + _fieldOfView / 2);
                if (hit.collider != null && hit.collider.gameObject.tag == other.tag)
                {
                    // Debug.Log("do see " + other.tag);
                    Debug.DrawRay(transform.position, toOther * _eyeDistance, Color.red);
                    eyeDetectOther = true;
                }
                else
                {
                    Debug.DrawRay(transform.position, toOther * _eyeDistance, Color.blue);
                    // Debug.Log("do NOT see " + other.tag);
                }
            }
        }
        return eyeDetectOther;
    }
}
