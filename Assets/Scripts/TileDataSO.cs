using UnityEngine;

[CreateAssetMenu(fileName = "NewTileData", menuName = "Frontline/Grid/Tile Data")]
public class TileDataSO : ScriptableObject
{
    [Header("Visual & Identification")]
    public string tileName;
    public BiomeType biomeType;
    public GameObject tilePrefab;

    [Header("Structural Stats")]
    [Tooltip("Determines if the tile can plummet into the abyss.")]
    public bool isFloating = true;

    [Tooltip("The starting health/durability of the tile's physical structure.")]
    [Range(0, 100)]
    public float maxStability = 100f;

    [Tooltip("How quickly this tile loses stability when unpowered.")]
    public float decayRate = 5.0f;

    [Header("Interaction Settings")]
    public bool canSupportTurrets = true;
    public float energyTransferEfficiency = 1.0f;

    private void OnValidate()
    {
        if (isFloating && decayRate <= 0f)
        {
            decayRate = 0.1f;
        }
    }
}

public enum BiomeType
{
    Wilderness,
    VitrifiedCrag,
    ObsidianFracture,
    GeothermalAbyssEdge
}
