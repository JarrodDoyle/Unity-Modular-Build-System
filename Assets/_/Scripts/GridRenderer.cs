using System;
using System.Collections.Generic;
using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    [SerializeField] private int gridDimensions;
    [SerializeField] private float cellSize;
    [SerializeField] private Material lineMaterial;
    [SerializeField] private Color color;

    private GameObject _gridLines;
    private bool _dirtyGrid;
    private List<LineRenderer> _lineRenderers;
    private Vector3 _prevCell;

    private GameObject _hitIndicator;

    private void Start()
    {
        _gridLines = new GameObject("Grid Lines");
        _gridLines.transform.SetParent(transform);

        _lineRenderers = new List<LineRenderer>();
        ResetGrid();

        _hitIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _hitIndicator.transform.localScale = Vector3.one * 0.25f;
    }

    private void Update()
    {
        if (_dirtyGrid)
        {
            ResetGrid();
            _dirtyGrid = false;
        }

        if (Input.GetKeyDown(KeyCode.Q)) MoveLines(new Vector3(0, -1, 0));
        if (Input.GetKeyDown(KeyCode.E)) MoveLines(new Vector3(0, 1, 0));

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var plane = new Plane(Vector3.up, -_prevCell.y * cellSize);
        if (plane.Raycast(ray, out var enter))
        {
            var hitPoint = ray.GetPoint(enter);
            _hitIndicator.transform.position = hitPoint;
            Vector3 newCell = Vector3Int.FloorToInt(hitPoint / cellSize);
            newCell.y = _prevCell.y;

            var dir = newCell - _prevCell;
            if (dir != Vector3.zero)
            {
                MoveLines(dir);
            }

            if (Input.GetMouseButtonDown(0))
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.transform.position = newCell * cellSize + Vector3.one * cellSize / 2;
            }
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            _dirtyGrid = true;
        }
    }

    private void ResetGrid()
    {
        foreach (var lr in _lineRenderers)
        {
            Destroy(lr.gameObject);
        }

        _lineRenderers.Clear();
        var lineLength = cellSize * gridDimensions;
        for (var i = 0; i <= gridDimensions; i++)
        {
            _lineRenderers.Add(SetupLineRenderer(
                new GameObject("Grid Line"), new Vector3(0, 0.1f, i * cellSize),
                new Vector3(lineLength, 0.1f, i * cellSize)));
            _lineRenderers.Add(SetupLineRenderer(
                new GameObject("Grid Line"), new Vector3(i * cellSize, 0.1f, 0),
                new Vector3(i * cellSize, 0.1f, lineLength)));
        }

        var cellOffset = -Mathf.FloorToInt(gridDimensions / 2f);
        MoveLines(new Vector3(cellOffset, 0, cellOffset));

        _prevCell = Vector3.zero;
    }

    private void MoveLines(Vector3 direction)
    {
        foreach (var lr in _lineRenderers)
        {
            lr.SetPosition(0, lr.GetPosition(0) + direction * cellSize);
            lr.SetPosition(1, lr.GetPosition(1) + direction * cellSize);
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