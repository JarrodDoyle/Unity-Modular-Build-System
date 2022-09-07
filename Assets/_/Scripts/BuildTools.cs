using System;
using System.Collections.Generic;
using UnityEngine;

public enum ToolType
{
    Select,
    Place,
    Remove
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
    private ToolType ToolType;
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
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetToolType((int) ToolType.Select);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetToolType((int) ToolType.Place);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetToolType((int) ToolType.Remove);

        switch (ToolType)
        {
            case ToolType.Select:
                SelectTool();
                break;
            case ToolType.Place:
                PlaceTool();
                break;
            case ToolType.Remove:
                RemoveTool();
                break;
        }
    }

    private void SelectTool()
    {
    }

    private void PlaceTool()
    {
        var cell = _gridState.HighlightCell;
        var cellBlocked = _gridObjectsMap.ContainsKey(cell);

        // Set primitive type
        if (Input.GetKeyDown(KeyCode.Z)) _primitiveType = PrimitiveType.Cube;
        if (Input.GetKeyDown(KeyCode.X)) _primitiveType = PrimitiveType.Capsule;
        if (Input.GetKeyDown(KeyCode.C)) _primitiveType = PrimitiveType.Cylinder;
        if (Input.GetKeyDown(KeyCode.V)) _primitiveType = PrimitiveType.Sphere;

        // Set rotation
        if (Input.GetKeyDown(KeyCode.R)) _rotation += 1;

        // Place a primitive
        if (Input.GetMouseButtonDown(0) && !cellBlocked)
        {
            var go = GameObject.CreatePrimitive(_primitiveType);
            go.transform.position = _gridState.cellSize * (cell + Vector3.one / 2);
            go.transform.rotation = Quaternion.Euler(0, 90f * (int) _rotation, 0);
            go.transform.SetParent(_gridObjectsManager.transform);
            go.layer = LayerMask.NameToLayer("Building");
            _gridObjectsMap.Add(cell, go);
        }
    }

    private void RemoveTool()
    {
        var cell = _gridState.CurrentCell;
        var cellBlocked = _gridObjectsMap.ContainsKey(cell);

        // Delete a primitive
        if (Input.GetMouseButtonDown(0) && cellBlocked)
        {
            var go = _gridObjectsMap[cell];
            Destroy(go);
            _gridObjectsMap.Remove(cell);
        }
    }

    public void SetToolType(int toolType)
    {
        ToolType = (ToolType) toolType;
        Debug.Log($"Set tool type: {ToolType}");
    }

    public void SetPrimitiveType(int primitiveType)
    {
        _primitiveType = (PrimitiveType) primitiveType;
        Debug.Log($"Set primitive type: {_primitiveType}");
    }
}