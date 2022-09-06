using System.Collections.Generic;
using UnityEngine;

public enum ToolType
{
    Place,
    Edit
}

public enum BlockRotation
{
    Up,
    Right,
    Down,
    Left,
}

public class BuildTools : MonoBehaviour
{
    private GridState _gridState;
    private GameObject _gridObjectsManager;
    private Dictionary<Vector3, GameObject> _gridObjectsMap;
    private PrimitiveType _primitiveType;
    private BlockRotation _rotation;

    private void Start()
    {
        _gridState = GetComponent<GridState>();
        _gridObjectsManager = new GameObject("Built Objects");
        _gridObjectsManager.transform.SetParent(transform);
        _gridObjectsMap = new Dictionary<Vector3, GameObject>();
        _primitiveType = PrimitiveType.Cube;
    }

    private void Update()
    {
        // Set primitive type
        if (Input.GetKeyDown(KeyCode.Alpha1)) _primitiveType = PrimitiveType.Cube;
        if (Input.GetKeyDown(KeyCode.Alpha2)) _primitiveType = PrimitiveType.Capsule;
        if (Input.GetKeyDown(KeyCode.Alpha3)) _primitiveType = PrimitiveType.Cylinder;
        if (Input.GetKeyDown(KeyCode.Alpha4)) _primitiveType = PrimitiveType.Sphere;

        // Set rotation
        if (Input.GetKeyDown(KeyCode.R)) _rotation += 1;

        var cell = _gridState.CurrentCell;
        var cellBlocked = _gridObjectsMap.ContainsKey(cell);

        // Place a primitive
        if (Input.GetMouseButtonDown(0) && !cellBlocked)
        {
            var go = GameObject.CreatePrimitive(_primitiveType);
            go.transform.position = _gridState.cellSize * (cell + Vector3.one / 2);
            go.transform.rotation = Quaternion.Euler(0, 90f * (int) _rotation, 0);
            go.transform.SetParent(_gridObjectsManager.transform);
            _gridObjectsMap.Add(cell, go);
        }

        // Delete a primitive
        if (Input.GetMouseButtonDown(1) && cellBlocked)
        {
            var go = _gridObjectsMap[cell];
            Destroy(go);
            _gridObjectsMap.Remove(cell);
        }
    }
}