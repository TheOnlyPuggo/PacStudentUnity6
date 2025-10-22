using System;
using UnityEngine;

public interface ITeleportable 
{
    GameObject ShadowReference { get; }
    bool IsInTeleport { get; set; }
}
