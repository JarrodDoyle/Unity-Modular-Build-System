using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
    [SerializeField] private BuildObject[] buildObjects;

    private ToolType _toolType;
    private GridState _gridState;
    private GameObject _gridObjectsManager;
    private Dictionary<Vector3, GridItem> _gridObjectsMap;
    private BuildObject _buildObject;
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
        _gridObjectsMap = new Dictionary<Vector3, GridItem>();
        _indicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _indicator.transform.SetParent(transform);
        _indicator.name = "Selection Indicator";
        _indicatorRenderer = _indicator.GetComponent<Renderer>();
        _indicatorRenderer.material = indicatorMaterial;
        SetToolType((int) ToolType.Select);
        SetBuildObject(0);
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
        if (_overUi) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (GetSelectedBuildObject(out var selected))
            {
                Debug.Log($"Selected object at {selected.cell}, slot {selected.slot}.");

                _selected = _gridObjectsMap[selected.cell].Slots[selected.slot].gameObject;
                _selectedPos = selected.cell;
            }
            else
            {
                _selected = null;
                Debug.Log($"No valid object at {_gridState.CurrentCell}.");
            }
        }

        // We want to try and move the selected object
        if (_selected == null) return;
        if (Input.GetMouseButton(0) && _selectedPos != _gridState.CurrentCell)
        {
            var moved = TryMoveBuildObject(_gridState.CurrentCell, _selected.transform);
            if (!moved) TryMoveBuildObject(_gridState.HighlightCell, _selected.transform);
        }

        // Rotate selection
        if (Input.GetKeyDown(KeyCode.R))
        {
            var buildObjSettings = _selected.GetComponent<BuildObjectSettings>();
            var gridItem = _gridObjectsMap[buildObjSettings.cell];
            var newSlot = (GridItemSlot) ((int) (buildObjSettings.slot + 1) % 4);
            if (gridItem.Slots[newSlot] == null)
            {
                gridItem.Slots[buildObjSettings.slot] = null;
                gridItem.Slots[newSlot] = _selected.transform;
                buildObjSettings.slot = newSlot;
                _rotation = (BlockRotation) newSlot;
                _selected.transform.rotation = Quaternion.Euler(0, 90f * (int) _rotation, 0);
            }
        }
    }

    private bool TryMoveBuildObject(Vector3 cell, Transform buildObj)
    {
        var buildObjSettings = buildObj.GetComponent<BuildObjectSettings>();
        var moved = false;
        if (_gridObjectsMap.ContainsKey(cell))
        {
            var gridItem = _gridObjectsMap[cell];
            if (gridItem.Slots[(GridItemSlot) _rotation] == null)
            {
                gridItem.Slots[(GridItemSlot) _rotation] = buildObj;
                moved = true;
            }
        }
        else
        {
            _gridObjectsMap[cell] = new GridItem {Slots = {[(GridItemSlot) _rotation] = buildObj}};
            moved = true;
        }

        if (moved)
        {
            // Remove the build object from it's previous slot
            var oldGridItem = _gridObjectsMap[buildObjSettings.cell];
            oldGridItem.Slots[buildObjSettings.slot] = null;
            if (oldGridItem.Slots.Values.All(v => v == null))
            {
                _gridObjectsMap.Remove(buildObjSettings.cell);
            }

            // Update the build object cell and world position
            buildObjSettings.cell = cell;
            _selectedPos = cell;
            buildObj.position = _gridState.cellSize * (cell + Vector3.one / 2);
        }

        return moved;
    }

    private void PlaceTool()
    {
        _indicator.transform.position = _gridState.cellSize * (_gridState.CurrentCell + Vector3.one / 2);

        // Set primitive type
        if (Input.GetKeyDown(KeyCode.Z)) SetBuildObject(0);
        if (Input.GetKeyDown(KeyCode.X)) SetBuildObject(1);
        if (Input.GetKeyDown(KeyCode.C)) SetBuildObject(2);
        if (Input.GetKeyDown(KeyCode.V)) SetBuildObject(3);

        // Set rotation
        if (Input.GetKeyDown(KeyCode.R)) _rotation = (BlockRotation) ((int) (_rotation + 1) % 4);

        // Attempt to place a build object
        if (!_overUi && Input.GetMouseButtonDown(0))
        {
            var placed = TryPlaceBuildObject(_gridState.CurrentCell);
            if (!placed) TryPlaceBuildObject(_gridState.HighlightCell);
        }
    }

    private bool TryPlaceBuildObject(Vector3 cell)
    {
        if (_gridObjectsMap.ContainsKey(cell))
        {
            var gridItem = _gridObjectsMap[cell];
            if (gridItem.Slots[(GridItemSlot) _rotation] == null)
            {
                gridItem.Slots[(GridItemSlot) _rotation] = InstantiateBuildObject(cell);
                return true;
            }
        }
        else
        {
            var gridItem = new GridItem {Slots = {[(GridItemSlot) _rotation] = InstantiateBuildObject(cell)}};
            _gridObjectsMap[cell] = gridItem;
            return true;
        }

        return false;
    }

    private Transform InstantiateBuildObject(Vector3 cell)
    {
        var layerIndex = LayerMask.NameToLayer("Building");
        var buildObject = Instantiate(_buildObject.prefab, _gridObjectsManager.transform, true);
        buildObject.position = _gridState.cellSize * (cell + Vector3.one / 2);
        buildObject.rotation = Quaternion.Euler(0, 90f * (int) _rotation, 0);
        buildObject.gameObject.layer = layerIndex;
        for (var i = 0; i < buildObject.childCount; i++)
        {
            var child = buildObject.GetChild(i);
            child.gameObject.layer = layerIndex;
        }

        var bos = buildObject.AddComponent<BuildObjectSettings>();
        bos.cell = cell;
        bos.slot = (GridItemSlot) _rotation;

        return buildObject;
    }

    private void RemoveTool()
    {
        _indicator.transform.position = _gridState.cellSize * (_gridState.CurrentCell + Vector3.one / 2);
        if (!_overUi && Input.GetMouseButtonDown(0) && GetSelectedBuildObject(out var settings))
        {
            var cell = settings.cell;
            var slot = settings.slot;
            var gridItem = _gridObjectsMap[cell];
            gridItem.Slots[slot] = null;
            Destroy(settings.gameObject);
            if (gridItem.Slots.Values.All(v => v == null))
            {
                _gridObjectsMap.Remove(cell);
            }
        }
    }

    private bool GetSelectedBuildObject(out BuildObjectSettings selected)
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hitInfo, _gridState.pickDistance, LayerMask.GetMask("Building")))
        {
            var hitTransform = hitInfo.transform;
            selected = hitTransform.GetComponentInParent<BuildObjectSettings>();
            return selected != null;
        }

        selected = null;
        return false;
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

    private void SetBuildObject(int index)
    {
        _buildObject = buildObjects[index];
        Debug.Log($"Set build object type: {_buildObject}");
    }
}