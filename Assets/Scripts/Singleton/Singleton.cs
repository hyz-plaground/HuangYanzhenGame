using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton Tool Class. Any singleton class intended should inherit this class.
/// </summary>
/// <typeparam name="T"> The class to inherit this singleton base class.</typeparam>
public class Singleton<T> where T : class, new()
{
    private static T _instance;

    public static T Instance
    {
        get { return _instance ??= new T(); }
    }
    
}