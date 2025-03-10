using System.Collections;
using System.Collections.Generic;
using KVA.SoundManager;
using UnityEngine;
using UnityEngine.Events;

// handles Player input and movement
[RequireComponent(typeof(PlayerAnimation))]
public class PlayerController : MonoBehaviour
{

    //  time to move one unit
    [Range(0.25f, 2f)]
    [SerializeField] private float _moveTime = 0.5f;

    // click indicator
    [SerializeField] Cursor _cursor;

    // cursor AnimationController
    private Animator _cursorAnimController;

    // pathfinding fields
    private Clickable[] _clickables;
    private Pathfinder _pathfinder;
    private Graph _graph;
    private Node _currentNode;
    private Node _nextNode;

    // flags
    private bool _isMoving;
    private bool _isControlEnabled;
    private PlayerAnimation _playerAnimation;

    private void Awake()
    {
        //  initialize fields
        _clickables = FindObjectsOfType<Clickable>();
        _pathfinder = FindObjectOfType<Pathfinder>();
        _playerAnimation = GetComponent<PlayerAnimation>();

        if (_pathfinder != null)
        {
            _graph = _pathfinder.GetComponent<Graph>();
        }

        _isMoving = false;
        _isControlEnabled = true;
    }

    private void Start()
    {
        // always start on a Node
        SnapToNearestNode();

        // automatically set the Graph's StartNode 
        if (_pathfinder != null && !_pathfinder.SearchOnStart)
        {
            _pathfinder.SetStartNode(transform.position);
        }

        //listen to all clickEvents
        foreach (Clickable c in _clickables)
        {
            c.clickAction += OnClick;
        }

        DragSpinner[] spinners = FindObjectsOfType<DragSpinner>();
        foreach (var spinner in spinners)
        {
            spinner.onSpinnerDragged.AddListener(StopAtNearestNode);
        }
    }

    private void OnDisable()
    {
        // unsubscribe from clickEvents when disabled
        foreach (Clickable c in _clickables)
        {
            c.clickAction -= OnClick;
        }
    }

    private void StopAtNearestNode()
    {
        if (_isMoving)
        {
            StopAllCoroutines();
            _isMoving = false;
            UpdateAnimation();
        }
    }

    private void OnClick(Clickable clickable, Vector3 position)
    {
        if (!_isControlEnabled || _isMoving || clickable == null || _pathfinder == null)
        {
            return;
        }

        SoundManager.PlaySound(SoundType.POP);

        // find the best path to the any Nodes under the Clickable; gives the user some flexibility
        List<Node> newPath = _pathfinder.FindBestPath(_currentNode, clickable.ChildNodes);

        // show a marker for the mouse click
        if (_cursor != null)
        {
            _cursor.ShowCursor(position);
        }

        // if we have a valid path, follow it
        if (newPath.Count > 1)
        {
            StartCoroutine(FollowPathRoutine(newPath));
        }
        else
        {
            // otherwise, invalid path, stop movement
            _isMoving = false;
            UpdateAnimation();
        }
    }



    private IEnumerator FollowPathRoutine(List<Node> path)
    {
        // start moving
        _isMoving = true;

        if (path == null || path.Count <= 1)
        {
            Debug.Log("PLAYERCONTROLLER FollowPathRoutine: invalid path");
        }
        else
        {
            UpdateAnimation();

            // loop through all Nodes
            for (int i = 1; i < path.Count; i++)
            {
                // use the current Node as the next waypoint
                _nextNode = path[i];

                // aim at the Node after that to minimize flipping
                int nextAimIndex = Mathf.Clamp(i + 1, 0, path.Count - 1);
                Node aimNode = path[nextAimIndex];
                FaceNextPosition(transform.position, aimNode.transform.position);

                // move to the next Node
                yield return StartCoroutine(MoveToNodeRoutine(transform.position, _nextNode));
            }
        }

        _isMoving = false;
        UpdateAnimation();

    }

    //  lerp to another Node from current position
    private IEnumerator MoveToNodeRoutine(Vector3 startPosition, Node targetNode)
    {

        float elapsedTime = 0;

        // validate move time
        _moveTime = Mathf.Clamp(_moveTime, 0.1f, 5f);

        while (elapsedTime < _moveTime && targetNode != null && !HasReachedNode(targetNode))
        {

            elapsedTime += Time.deltaTime;
            float lerpValue = Mathf.Clamp(elapsedTime / _moveTime, 0f, 1f);

            Vector3 targetPos = targetNode.transform.position;
            transform.position = Vector3.Lerp(startPosition, targetPos, lerpValue);

            // if over halfway, change parent to next node
            if (lerpValue > 0.51f)
            {
                transform.parent = targetNode.transform;
                _currentNode = targetNode;

                // invoke UnityEvent associated with next Node
                targetNode.gameEvent.Invoke();
                //Debug.Log("invoked GameEvent from targetNode: " + targetNode.name);
            }

            // wait one frame
            yield return null;
        }

        SoundManager.PlaySound(SoundType.FOOTSTEP);
    }

    // snap the Player to the nearest Node in Game view
    public void SnapToNearestNode()
    {
        Node nearestNode = _graph?.FindClosestNode(transform.position);
        if (nearestNode != null)
        {
            _currentNode = nearestNode;
            transform.position = nearestNode.transform.position;
        }
    }

    // turn face the next Node, always projected on a plane at the Player's feet
    public void FaceNextPosition(Vector3 startPosition, Vector3 nextPosition)
    {
        if (Camera.main == null)
        {
            return;
        }

        // convert next Node world space to screen space
        Vector3 nextPositionScreen = Camera.main.WorldToScreenPoint(nextPosition);

        // convert next Node screen point to Ray
        Ray rayToNextPosition = Camera.main.ScreenPointToRay(nextPositionScreen);

        // plane at player's feet
        Plane plane = new Plane(Vector3.up, startPosition);

        // distance from camera (used for projecting point onto plane)
        float cameraDistance = 0f;

        // project the nextNode onto the plane and face toward projected point
        if (plane.Raycast(rayToNextPosition, out cameraDistance))
        {
            Vector3 nextPositionOnPlane = rayToNextPosition.GetPoint(cameraDistance);
            Vector3 directionToNextNode = nextPositionOnPlane - startPosition;
            if (directionToNextNode != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(directionToNextNode);
            }
        }
    }

    // toggle between Idle and Walk animations
    private void UpdateAnimation()
    {
        if (_playerAnimation != null)
        {
            _playerAnimation.ToggleAnimation(_isMoving);
        }
    }

    // have we reached a specific Node?
    public bool HasReachedNode(Node node)
    {
        if (_pathfinder == null || _graph == null || node == null)
        {
            return false;
        }

        float distanceSqr = (node.transform.position - transform.position).sqrMagnitude;

        return (distanceSqr < 0.01f);
    }

    // have we reached the end of the graph?
    public bool HasReachedGoal()
    {
        if (_graph == null)
        {
            return false;
        }

        return HasReachedNode(_graph.GoalNode);
    }

    //  enable/disable controls
    public void EnableControls(bool state)
    {
        _isControlEnabled = state;
    }
}
