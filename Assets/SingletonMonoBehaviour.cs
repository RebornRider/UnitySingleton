using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>   Exception for signaling multiple singleton instance errors. </summary>
internal class MultipleSingletonInstanceException : Exception
{
    /// <summary>   Constructor. </summary>
    ///
    /// <param name="message">  The error message.
    ///                         </param>
    public MultipleSingletonInstanceException(string message) : base(message)
    {
    }
}

/// <summary>   A singleton MonoBehaviour. </summary>
///
/// <typeparam name="T">    Generic type parameter.
///                         </typeparam>
public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
{
    /// <summary>   Gets or sets the instance. </summary>
    ///
    /// <value> The instance. </value>
    public static T Instance { get; private set; }

    /// <summary>   Awakes this object. </summary>
    protected virtual void Awake()
    {
        ValidateSingletonInBuild();
    }

    /// <summary>   Validates the singleton in build. </summary>
    ///
    /// <exception cref="MultipleSingletonInstanceException">   Thrown when a Multiple Singleton
    ///                                                         Instance error if this is duplicate instance occurs.
    ///                                                         </exception>
    private void ValidateSingletonInBuild()
    {
        if (null != Instance)
        {
            Destroy(this);
            throw new MultipleSingletonInstanceException("Amount of Instances of " + typeof(T).Name + " more then one");
        }
        Instance = (T)this;
    }

    /// <summary>   Unassigns Instance on destruction. </summary>
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>   Resets this object. Called when adding a component in Editor or when Reseting a component in editor. Validates Singleton character. </summary>
    protected virtual void Reset()
    {
#if UNITY_EDITOR
        if (Application.isEditor && false == Application.isPlaying)
        {
            ValidateSingletonInEditor();
            return;
        }
#endif
        ValidateSingletonInBuild();
    }


#if UNITY_EDITOR
    /// <summary>   Validates the Singleton in editor. </summary>
    private static void ValidateSingletonInEditor()
    {
        ICollection<T> instances = Resources.FindObjectsOfTypeAll<T>()
            .Where(instance => false == EditorUtility.IsPersistent(instance))
            .ToList();

        if (instances.Count <= 1)
        {
            return;
        }

        HandleMultipleInstances(instances);
    }

    /// <summary>   Handles the multiple instances in scene described by instances. </summary>
    ///
    /// <exception cref="MultipleSingletonInstanceException">   Thrown after warning dialog ends.
    ///                                                         </exception>
    ///
    /// <param name="instances">    The instances.
    ///                             </param>
    private static void HandleMultipleInstances(ICollection<T> instances)
    {
        string instanceTrace = GenerateInstanceTrace(instances);

        bool logTrace = ShowWarning(instanceTrace, instances.Count);

        if (logTrace)
        {
            Debug.Log(instanceTrace);
        }

        throw new MultipleSingletonInstanceException("Multiple Instances of " + typeof(T).Name);
    }

    /// <summary>   Generates an instance trace in hierarchy. </summary>
    ///
    /// <param name="instances">    The instances.
    ///                             </param>
    ///
    /// <returns>   The instance trace. </returns>
    private static string GenerateInstanceTrace(IEnumerable<T> instances)
    {
        string seperator = "\t " + new string('-', 50);

        var sb = new StringBuilder();

        sb.Append("Instance Trace:\n");
        sb.Append(seperator);

        foreach (T instance in instances)
        {
            sb.Append(string.Format("\n\tGameobject: {0}\n\tPath: {1}\n{2}",
                instance.name, GetHierarchyPath(instance.transform), seperator));
        }

        return sb.ToString();
    }

    /// <summary>   Gets hierarchy path. </summary>
    ///
    /// <param name="current">  The current path step.
    ///                         </param>
    ///
    /// <returns>   The hierarchy path. </returns>
    private static string GetHierarchyPath(Transform current)
    {
        if (current.parent == null)
            return "/" + current.name;
        return GetHierarchyPath(current.parent) + "/" + current.name;
    }

    /// <summary>   Shows the warning. </summary>
    ///
    /// <param name="instanceTrace">         The intance trace.
    ///                                     </param>
    /// <param name="numberOfInstances">    Number of instances.
    ///                                     </param>
    ///
    /// <returns>   true if it succeeds, false if it fails. </returns>
    private static bool ShowWarning(string instanceTrace, int numberOfInstances)
    {
        bool result = EditorUtility.DisplayDialog(
            GenerateWarningTitle(),
            GenerateWarningMessage(instanceTrace, numberOfInstances),
            "OK. Log Instance Trace.",
            "Cancel"
            );
        return result;
    }

    /// <summary>   Generates the warning title. </summary>
    ///
    /// <returns>   The warning title. </returns>
    private static string GenerateWarningTitle()
    {
        return string.Format("To many instances of {0}", typeof(T).Name);
    }

    /// <summary>   Generates the warning message. </summary>
    ///
    /// <param name="instanceTrace">          The instance text.
    ///                                     </param>
    /// <param name="numberOfInstances">    Number of instances.
    ///                                     </param>
    ///
    /// <returns>   The warning message. </returns>
    private static string GenerateWarningMessage(string instanceTrace, int numberOfInstances)
    {
        var sb = new StringBuilder();
        sb.Append(numberOfInstances);
        sb.Append(" Instances of ");
        sb.Append(typeof(T).Name);
        sb.Append(" found in scene.\n");
        sb.Append("!!!!!Only one Instance should exist!!!!!\n\n");
        sb.Append(instanceTrace);
        return sb.ToString();
    }

#endif
}