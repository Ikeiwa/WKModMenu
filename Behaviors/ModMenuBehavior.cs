using System.Collections;
using UnityEngine;

namespace ModMenu.Behaviors;

public class ModMenuBehavior : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return null;
        ModMenu.Initialize(transform);
    }
}