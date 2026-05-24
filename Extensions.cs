using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModMenu;

public static class Extensions
{
    public static GameObject Search(this GameObject go, string search)
    {
        if(!go) return null;
        if (string.IsNullOrEmpty(search)) return null;

        Queue<string> path = new Queue<string>(search.Split('/', StringSplitOptions.RemoveEmptyEntries));
        Transform searched = go.transform;
        while (path.Count > 0)
        {
            string current = path.Dequeue();

            bool found = false;
            if (current.Equals("..") && searched.parent)
            {
                found = true;
                searched = searched.parent;
            }
            else
            {
                for (int i = 0; i < searched.childCount; i++)
                {
                    var child = searched.GetChild(i);
                    if (!current.Equals(child.gameObject.name)) continue;
                
                    searched = child;
                    found = true;
                    break;
                }
            }

            if (!found) return null;
        }

        return searched.gameObject;
    }
    
    public static GameObject Search(this Transform tr, string search)
    {
        if(!tr) return null;
        return tr.gameObject.Search(search);
    }
}