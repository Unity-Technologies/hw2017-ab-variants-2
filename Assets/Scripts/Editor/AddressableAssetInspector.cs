using System.Linq;
using System.Linq.Expressions;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System;

[InitializeOnLoad]
public class test
{
    static test()
    {
        BindToDelegate(Assembly.GetAssembly(typeof(UnityEditor.Editor)), "UnityEditor.GameObjectInspector", "OnDrawPostHeaderGUI", "test", "OnPostHeaderGUI");
    }

    private static bool BindToDelegate(Assembly delAssembly, string delClassName, string delegateName, string targetClassName, string targetMethodName)
    {
        try
        {
            var delType = delAssembly.GetType(delClassName, true);
            var delField = delType.GetField(delegateName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            var targetType = Type.GetType(targetClassName, true);
            var targetMethod = targetType.GetMethod(targetMethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            var del = Delegate.CreateDelegate(delField.FieldType, targetMethod);
            delField.SetValue(null, del);
            return true;

        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            return false;
        }
    }

    static GUIContent addressableAssetToggleText = new GUIContent("Addressable", "Check this to mark this asset as an Adressable Asset, which includes it in the bundled data and makes it loadable via script by its address.");
    [System.Flags]
    public enum AddressableAssetEntryFlags
    {
        Visible = 1 << 0,
        Build = 1 << 1,
    }


    static bool ValidatePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;
        if (path == "library/unity editor resources" ||
            path == "library/unity default resources" ||
            path == "Resources/unity_builtin_extra")
            return false;
        var ext = System.IO.Path.GetExtension(path).ToLower();
        if (ext == ".cs" || ext == ".js" || ext == ".boo" || ext == ".exe" || ext == ".dll")
            return false;
        return true;
    }


    static void SetEntryWithUndo(AddressableAssetSettings settings, AddressableAssetEntry entry)
    {
        Undo.RecordObject(settings, "AddressableAssetSettings");
        settings.SetEntry(entry);
    }

    static protected void OnPostHeaderGUI(Editor editor)
    {
        var settings = AddressableAssetSettings.GetDefault();

        var path = AssetDatabase.GetAssetOrScenePath(editor.target);
        if (!ValidatePath(path))
            return;

        var guidStr = AssetDatabase.AssetPathToGUID(path);
        if (string.IsNullOrEmpty(guidStr))
            return;
        var guid = new GUID(guidStr);

        var entry = settings.GetEntry(guid);
        GUILayout.BeginHorizontal();
        //Undo.RegisterCompleteObjectUndo(settings, "AddressableAssetSettings");
        if (GUILayout.Toggle(entry.active, addressableAssetToggleText, GUILayout.Width(120)))
        {
            if (!entry.active)
                SetEntryWithUndo(settings, entry = new AddressableAssetEntry(guid, entry.address, true, entry.flags));
        }
        else
        {
            if (entry.active)
                SetEntryWithUndo(settings, entry = new AddressableAssetEntry(guid, entry.address, false, entry.flags));
        }

        if (entry.active)
        {
            var displayAddress = string.IsNullOrEmpty(entry.address) ? AssetDatabase.GUIDToAssetPath(guidStr) : entry.address;
            var address = EditorGUILayout.DelayedTextField("", displayAddress);
            if (address != displayAddress)
                SetEntryWithUndo(settings, new AddressableAssetEntry(entry.guid, address == AssetDatabase.GUIDToAssetPath(guidStr) ? string.Empty : address, entry.active, entry.flags));
            var flags = (AddressableAssetEntryFlags)EditorGUILayout.EnumMaskField("Flags", (AddressableAssetEntryFlags)entry.flags);
            if (flags != (AddressableAssetEntryFlags)entry.flags)
                SetEntryWithUndo(settings, new AddressableAssetEntry(entry.guid, entry.address, entry.active, (int)flags));
        }
        GUILayout.EndHorizontal();
    }

}
