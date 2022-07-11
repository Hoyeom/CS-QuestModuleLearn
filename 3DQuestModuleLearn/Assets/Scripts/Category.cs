﻿using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Category",fileName = "Category_")]
public class Category : ScriptableObject, IEquatable<Category>
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;

    public string Id => id;
    public string DisplayName => displayName;


    #region Operator

    public bool Equals(Category other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(other, this))
            return true;
        if (GetType() != other.GetType())
            return false;

        return id == other.id;
    }

    public override int GetHashCode() => (Id, DisplayName).GetHashCode();

    public override bool Equals(object other) => base.Equals(other);

    public static bool operator ==(Category lhs, string rhs)
    {
        if (lhs is null)
            return ReferenceEquals(rhs, null);
        return lhs.Id == rhs || lhs.DisplayName == rhs;
    }

    public static bool operator !=(Category lhs, string rhs) => !(lhs == rhs);
    
    // category.CodeName == "Kill"
    // category == "Kill"


    #endregion
}