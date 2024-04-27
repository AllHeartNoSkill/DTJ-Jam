using Godot;
using System;

namespace DTJJam.Scripts.Levels;

public partial class LevelObjectData : Resource
{
    [Export] public float ObjectGridSize;
    [Export] public PackedScene LevelObjectPrefab;

    public LevelObjectData()
    {
        ObjectGridSize = 1f;
    }
}