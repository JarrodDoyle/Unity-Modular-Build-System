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

    private GridState _gridState;

    private void Start()
    {
        _gridState = GetComponent<GridState>();

        _gridLines = new GameObject("Grid Lines");
        _gridLines.transform.SetParent(transform);

        _lineRenderers = new List<LineRenderer>();
        ResetGrid();
    }

    private void Update()
    {
        if (dirtyGrid)
        {
            dirtyGrid = false;
            ResetGrid();
            _gridLines.SetActive(_gridState.showGrid);
        }

        var dir = _gridState.HighlightCell - _prevCell;
        if (dir != Vector3.zero)
        {
            MoveLines(dir);
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
        var lineLength = _gridState.cellSize * _gridState.gridDimensions;
        for (var i = 0; i <= _gridState.gridDimensions; i++)
        {
            var offset = i * _gridState.cellSize;
            _lineRenderers.Add(SetupLineRenderer(
                new GameObject("Grid Line"), new Vector3(0, 0.1f, offset),
                new Vector3(lineLength, 0.1f, offset)));
            _lineRenderers.Add(SetupLineRenderer(
                new GameObject("Grid Line"), new Vector3(offset, 0.1f, 0),
                new Vector3(offset, 0.1f, lineLength)));
        }

        var cellOffset = -Mathf.FloorToInt(_gridState.gridDimensions / 2f);
        MoveLines(new Vector3(cellOffset, 0, cellOffset));

        _prevCell = Vector3.zero;
    }

    private void MoveLines(Vector3 direction)
    {
        var offset = direction * _gridState.cellSize;
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