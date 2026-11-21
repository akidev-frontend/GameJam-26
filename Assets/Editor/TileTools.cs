#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public static class TileTools
{
    // Vuelca a la consola qué tile hay en cada celda del Tilemap del nivel
    [MenuItem("Tools/Dump Tilemap Level2")]
    public static void DumpTilemap()
    {
        var grid = Object.FindFirstObjectByType<Grid>();
        var tm = grid.GetComponentInChildren<Tilemap>();
        Debug.Log("TILEMAP " + tm.name + " bounds " + tm.cellBounds);
        for (int y = tm.cellBounds.yMin; y <= tm.cellBounds.yMax; y++)
        {
            for (int x = tm.cellBounds.xMin; x <= tm.cellBounds.xMax; x++)
            {
                var t = tm.GetTile(new Vector3Int(x, y, 0));
                if (t != null) Debug.Log($"TILE {x},{y} : {t.name}");
            }
        }
        Debug.Log("DUMP_END");
    }
}
#endif