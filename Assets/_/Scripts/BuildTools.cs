using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
    [SerializeField] private Material indicatorMaterial;
    [SerializeField] private Color placeIndicatorColor;
    [SerializeField] private Color removeIndicatorColor;

    private ToolType _toolType;
    private GridState _gridState;
    private GameObject _gridObjectsManager;
    private Dictionary<Vector3, GameObject> _gridObjectsMap;
    private PrimitiveType _primitiveType;
    private BlockRotation _rotation;
    private GameObject _indicator;
    private Renderer _indicatorRenderer;
    private bool _overUi;
    private GameObject _selected;
    private Vector3 _selectedPos;

    private void Start()
    {
        _gridState = GetComponent<GridState>();
        _gridObjectsManager = new GameObject("Built Objects");
        _gridObjectsManager.transform.SetParent(transform);
        _gridObjectsMap = new Dictionary<Vector3, GameObject>();
        _primitiveType = PrimitiveType.Cube;
        _indicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _indicator.transform.SetParent(transform);
        _indicator.name = "Selection Indicator";
        _indicatorRenderer = _indicator.GetComponent<Renderer>();
        _indicatorRenderer.material = indicatorMaterial;
        SetToolType((int) ToolType.Select);
    }

    private void Update()
    {
        var selected = EventSystem.current.currentSelectedGameObject;
        _overUi = selected != null && selected.layer == LayerMask.NameToLayer("UI");
        
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetToolType((int) ToolType.Select);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetToolType((int) ToolType.Place);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetToolType((int) ToolType.Remove);

        switch (_toolType)
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
        var cell = _gridState.CurrentCell;
        var cellBlocked = _gridObjectsMap.ContainsKey(cell);
        _indicator.transform.position = _gridState.cellSize * (cell + Vector3.one / 2);

        if (cellBlocked && Input.GetMouseButtonDown(0))
        {
            Debug.Log($"Selected object at {cell}.");
            _selected = _gridObjectsMap[cell];
            _selectedPos = cell;
        } else if (Input.GetMouseButtonDown(0))
        {
            Debug.Log($"No valid object at {cell}.");
        }

        if (_selected == null) return;

        // We want to try and move the selected object
        if (Input.GetMouseButton(0) && _selectedPos != cell)
        {
            var move = false;
            var moveCell = cell;
            var hlCell = _gridState.HighlightCell;
            var hlCellBlocked = _gridObjectsMap.ContainsKey(hlCell);
            
            if (!cellBlocked)
            {
                move = true;
            }
            else if (!hlCellBlocked && _selectedPos != hlCell)
            {
                move = true;
                moveCell = hlCell;
            }

            if (move)
            {
                Debug.Log($"Moved object from {_selectedPos} to {moveCell}.");
                _gridObjectsMap.Remove(_selectedPos);
                _gridObjectsMap[moveCell] = _selected;
                _selectedPos = moveCell;
                _selected.transform.position = _gridState.cellSize * (moveCell + Vector3.one / 2);
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            var rotation = _selected.transform.rotation.eulerAngles.y;
            _selected.transform.rotation = Quaternion.Euler(0, rotation + 90f, 0);
        }
    }

    private void PlaceTool()
    {
        var cell = _gridState.HighlightCell;
        var cellBlocked = _gridObjectsMap.ContainsKey(cell);
        _indicator.transform.position = _gridState.cellSize * (cell + Vector3.one / 2);

        // Set primitive type
        if (Input.GetKeyDown(KeyCode.Z)) _primitiveType = PrimitiveType.Cube;
        if (Input.GetKeyDown(KeyCode.X)) _primitiveType = PrimitiveType.Capsule;
        if (Input.GetKeyDown(KeyCode.C)) _primitiveType = PrimitiveType.Cylinder;
        if (Input.GetKeyDown(KeyCode.V)) _primitiveType = PrimitiveType.Sphere;

        // Set rotation
        if (Input.GetKeyDown(KeyCode.R)) _rotation += 1;

        // Place a primitive
        if (!_overUi && Input.GetMouseButtonDown(0) && !cellBlocked)
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
        _indicator.transform.position = _gridState.cellSize * (cell + Vector3.one / 2);

        // Delete a primitive
        if (!_overUi && Input.GetMouseButtonDown(0) && cellBlocked)
        {
            var go = _gridObjectsMap[cell];
            Destroy(go);
            _gridObjectsMap.Remove(cell);
        }
    }

    public void SetToolType(int toolType)
    {
        _toolType = (ToolType) toolType;
        switch (_toolType)
        {
            case ToolType.Select:
                _indicator.SetActive(false);
                break;
            case ToolType.Place:
                _indicatorRenderer.material.color = placeIndicatorColor;
                _selected = null;
                _indicator.SetActive(true);
                break;
            case ToolType.Remove:
                _indicatorRenderer.material.color = removeIndicatorColor;
                _selected = null;
                _indicator.SetActive(true);
                break;
        }
        
        Debug.Log($"Set tool type: {_toolType}");
    }

    public void SetPrimitiveType(int primitiveType)
    {
        _primitiveType = (PrimitiveType) primitiveType;
        Debug.Log($"Set primitive type: {_primitiveType}");
    }
}