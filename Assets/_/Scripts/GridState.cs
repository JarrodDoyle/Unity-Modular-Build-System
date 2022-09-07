using System;
using System.Linq;
using UnityEngine;

public class GridState : MonoBehaviour
{
    public float cellSize;
    public float pickDistance;

    public Vector3 CurrentCell { get; private set; }

    public Vector3 HighlightCell { get; private set; }

    private void Update()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hitInfo, pickDistance, LayerMask.GetMask("Building", "Terrain")))
        {
            var hitLayer = hitInfo.transform.gameObject.layer;

            if (hitLayer == LayerMask.NameToLayer("Terrain"))
            {
                var hitPoint = hitInfo.point;
                Vector3 newCell = Vector3Int.FloorToInt(hitPoint / cellSize);
                CurrentCell = newCell;
                HighlightCell = CurrentCell;
            }
            else if (hitLayer == LayerMask.NameToLayer("Building"))
            {
                var hitPoint = hitInfo.transform.position;
                Vector3 newCell = Vector3Int.FloorToInt(hitPoint / cellSize);
                CurrentCell = newCell;

                // Calculate most dominant normal axis
                var normal = hitInfo.normal;
                var values = new[] {Mathf.Abs(normal.x), Mathf.Abs(normal.y), Mathf.Abs(normal.z)};
                var maxIndex = Array.IndexOf(values, values.Max());
                var dir = Vector3.zero;
                dir[maxIndex] = normal[maxIndex];
                dir.Normalize();
                HighlightCell = CurrentCell + dir;
            }
        }
    }
}