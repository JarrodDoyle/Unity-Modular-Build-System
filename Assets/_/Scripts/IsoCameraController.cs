using UnityEngine;

public class IsoCameraController : MonoBehaviour
{
    public float moveSpeed;
    public float rotationSpeed;
    [SerializeField] private GridState gridState;

    private readonly Vector3 _north = new(0, 0, 1);
    private readonly Vector3 _east = new(1, 0, 0);
    private readonly Vector3 _south = new(0, 0, -1);
    private readonly Vector3 _west = new(-1, 0, 0);

    private Vector3 _prevMousePos;
    private float _rotation = 45f;

    private void Update()
    {
        var position = transform.position;

        // Cardinal direction movement
        var dir = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) dir += _north;
        if (Input.GetKey(KeyCode.A)) dir += _west;
        if (Input.GetKey(KeyCode.S)) dir += _south;
        if (Input.GetKey(KeyCode.D)) dir += _east;
        if (dir != Vector3.zero) dir = dir.normalized;
        dir = Quaternion.Euler(0, _rotation, 0) * dir;
        position += dir * (moveSpeed * Time.deltaTime);
        
        // Vertical movement
        var scrollDelta = Input.mouseScrollDelta.y;
        if (scrollDelta != 0) position.y -= Mathf.Sign(scrollDelta) * gridState.cellSize;

        // Rotation
        var newMousePos = Input.mousePosition;
        if (Input.GetMouseButton(2))
        {
            var mouseDelta = (newMousePos - _prevMousePos).x;
            var rotationChange = mouseDelta * rotationSpeed * Time.deltaTime;
            transform.Rotate(Vector3.up, rotationChange, Space.World);
            _rotation += rotationChange;
            while (_rotation < 0)
                _rotation += 360;
            _rotation %= 360;
        }

        transform.position = position;
        _prevMousePos = newMousePos;
    }
}