using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Task/Target/String", fileName = "Target_")]
public class StringTarget : TaskTarget
{
    [SerializeField]
    string value;
    public override object Value => value;

    public override bool IsEqual(object target)
    {
        string targetAsString = target as string;

        return targetAsString == null ? false : value == targetAsString;
    }
}
