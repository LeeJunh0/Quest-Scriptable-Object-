using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InitialSccuessValue : ScriptableObject
{
    public abstract int GetValue(Task task);
}
