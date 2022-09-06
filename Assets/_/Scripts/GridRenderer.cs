using System.Collections.Generic;
using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    [SerializeField] private Material lineMaterial;
    [SerializeField] private Color color;

    [HideInInspector] public bool dirtyGrid;
    private GameObject _gridLines;
    private List<LineRenderer> _lineRenderers;
    private Vector3 _prevCell;

    private GameObject _hitIndicator;
    private GridSettings _gridSettings;

    private void Start()
    {
        _gridSettings = GetComponent<GridSettings>();

        _gridLines = new GameObject("Grid Lines");
        _gridLines.transform.SetParent(transform);

        _lineRenderers = new List<LineRenderer>();
        ResetGrid();

        _hitIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _hitIndicator.transform.localScale = Vector3.one * 0.25f;
    }

    private void Update()
    {
        if (dirtyGrid)
        {
            dirtyGrid = false;
            ResetGrid();
            _gridLines.SetActive(_gridSettings.showGrid);
        }

        if (Input.GetKeyDown(KeyCode.Q)) MoveLines(new Vector3(0, -1, 0));
        if (Input.GetKeyDown(KeyCode.E)) MoveLines(new Vector3(0, 1, 0));

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var plane = new Plane(Vector3.up, -_prevCell.y * _gridSettings.cellSize);
        if (plane.Raycast(ray, out var enter))
        {
            var hitPoint = ray.GetPoint(enter);
            _hitIndicator.transform.position = hitPoint;
            Vector3 newCell = Vector3Int.FloorToInt(hitPoint / _gridSettings.cellSize);
            newCell.y = _prevCell.y;

            var dir = newCell - _prevCell;
            if (dir != Vector3.zero)
            {
                MoveLines(dir);
            }

            if (Input.GetMouseButtonDown(0))
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.transform.position = _gridSettings.cellSize * (newCell + Vector3.one / 2);
            }
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            dirtyGrid = true;
        }
    }

    private void ResetGrid()
    {
        foreach (var lr in _lineRenderers)
        {
            Destroy(lr.gameObject);
        }

        _lineRenderers.Clear();
        var lineLength = _gridSettings.cellSize * _gridSettings.gridDimensions;
        for (var i = 0; i <= _gridSettings.gridDimensions; i++)
        {
            var offset = i * _gridSettings.cellSize;
            _lineRenderers.Add(SetupLineRenderer(
                new GameObject("Grid Line"), new Vector3(0, 0.1f, offset),
                new Vector3(lineLength, 0.1f, offset)));
            _lineRenderers.Add(SetupLineRenderer(
                new GameObject("Grid Line"), new Vector3(offset, 0.1f, 0),
                new Vector3(offset, 0.1f, lineLength)));
        }

        var cellOffset = -Mathf.FloorToInt(_gridSettings.gridDimensions / 2f);
        MoveLines(new Vector3(cellOffset, 0, cellOffset));

        _prevCell = Vector3.zero;
    }

    private void MoveLines(Vector3 direction)
    {
        var offset = direction * _gridSettings.cellSize;
        foreach (var lr in _lineRenderers)
        {
            lr.SetPosition(0, lr.GetPosition(0) + offset);
            lr.SetPosition(1, lr.GetPosition(1) + offset);
        }

        _prevCell += direction;
    }

    private LineRenderer SetupLineRenderer(GameObject go, Vector3 start, Vector3 end)
    {
        go.transform.SetParent(_gridLines.transform);
        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.widthMultiplier = 0.025f;
        lr.startColor = color;
        lr.endColor = color;
        lr.material = lineMaterial;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        return lr;
    }
}