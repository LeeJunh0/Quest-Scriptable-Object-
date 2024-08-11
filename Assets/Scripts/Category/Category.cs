using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Category",fileName = "Category_")]
public class Category : ScriptableObject, IEquatable<Category>
{
    [SerializeField]
    string codeName;
    [SerializeField]
    string displayName;

    public string CodeName => codeName;
    public string DisplayName => displayName;

    #region Operator
    public bool Equals(Category other)
    {
        if (other == null)
            return false;
        if (ReferenceEquals(other, this))
            return true;
        if (GetType() != other.GetType())
            return false;

        return codeName == other.codeName;
    }

    public override int GetHashCode() => (CodeName, DisplayName).GetHashCode();

    public override bool Equals(object other) => Equals(other as Category);

    public static bool operator==(Category lhs, string rhs)
    {
        if (lhs is null)
            return ReferenceEquals(rhs, null);
        return lhs.CodeName == rhs || lhs.DisplayName == rhs;
    }

    public static bool operator!=(Category lhs, string rhs) => !(lhs == rhs);
    #endregion
}
