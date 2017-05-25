using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using System;
using UnityEditor.Localization;
using UnityEngine.Localization;

[ScriptedImporter(1, "po")]
public class POImporter : ScriptedImporter
 {
	 public SystemLanguage referenceLanguage = SystemLanguage.English;
	 public bool updateReferenceLanguage = false;
    public override void OnImportAsset(AssetImportContext ctx)
    {
		var db = ScriptableObject.CreateInstance<MultiLangStringDatabase>();
		POUtility.ImportFile(db, ctx.assetPath, referenceLanguage, updateReferenceLanguage);
		ctx.SetMainAsset("PO File", db);
    }
}
