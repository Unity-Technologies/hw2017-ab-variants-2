using UnityEngine;
using UnityEngine.Localization;
using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine {
	namespace Localization {
		public class POUtility {

			//the po file headers that are required
			private static string CreatePoHeaderEntry (SystemLanguage language)
			{
				string newLang = language.ToString().Substring(0, 2).ToLower();

				string PoInitEntry = 

				"msgid " + '"' + '"' + "\n" +
				"msgstr "  + '"' + '"' + "\n" +
				'"' + "Project-Id-Version: " + '"' + "\n" + 
				'"' + "Report-Msgid-Bugs-To: " + '"' + "\n" + 
                '"' + "POT-Creation-Date: " + System.DateTime.Now.ToShortDateString() + "\\n" + '"' +  "\n" +
				'"' + "PO-Revision-Date: \\n" + '"' + "\n" +
				'"' + "Last-Translator: \\n" + '"' + "\n" +
				'"' + "Language-Team: \\n" + '"' + "\n" +
				'"' + "Language: " + newLang + "\\n" + '"' + "\n" +
				'"' + "MIME-Version: 1.0\\n" + '"' + "\n" +
				'"' + "Content-Type: text/plain; charset=UTF-8\\n" + '"' + "\n" +
				'"' + "Content-Transfer-Encoding: 8bit\\n" + '"' + "\n" +
                '"' + "X-Generator: Unity " + Application.unityVersion + "\\n" + '"' + "\n";
				return PoInitEntry;
			}

			//create the po file

			/// POs the entry.
			/// </summary>
			/// <returns>The PO entry.</returns>
			/// <param name="key">Key</param>
			/// <param name="msgid">Value to be translated</param>
			/// <param name="msgstr">Translated Value</param>
			/// <param name="comment">Production Comment</param>
			private static string CreatePOEntry(string msgid, string msgstr, string[] comments)
			{
				//check if msgid has quotes
				msgid.Replace("\"","\\\"");
				msgstr.Replace("\"","\\\"");

				string entry = "";

				if (comments != null) 
				{
					foreach (var c in comments)
					{
						entry += "#: " + c + "\n";
					}
				}

				//create entry
				entry +=
				//"msgctxt " + '"' + key + '"' + "\n" +
				"msgid " + '"' + msgid + '"' + "\n" +
				"msgstr " + '"' + msgstr + '"' + "\n" + "\n";
				
				return entry;
			}

			private class POEntry {

				public string msgid;        // Id of the string being translated.
				public string msgctxt = ""; // Disambiguating context. Allows for POEntries to have the same id value.
				public string msgstr;       // The translated string.
				public List<string> comments;

				public string Key
				{
					get
					{
						if(string.IsNullOrEmpty(msgctxt))
							return msgid;
						return msgid + "(" + msgctxt + ")";
					}
				}

				public POEntry() {
					comments = new List<string>();
				}

				public bool ready {
					get {
						return msgid != null && msgctxt != null && msgstr != null &&
							msgid.Length > 0;
					}
				}
			}

			private static SystemLanguage ReadPOHeader(StreamReader r) {

				SystemLanguage l = SystemLanguage.Unknown;

				// read header
				while(!r.EndOfStream) {
					string line = r.ReadLine();
					if(line == null ) {
						break;
					}
					line = line.Trim();
					if(line.Length == 0) {
						break;
					}

					if(line.Contains("Language:"))
					{
						line = line.Replace("Language:", "").Trim().Trim('\"').Replace("\\n", "").Trim();
						l = MultiLangEditorUtility.ISOToSystemLanguage(line);
					}
				}

				return l;
			}

			private static POEntry ReadNextEntry(StreamReader r) {

				POEntry e = null;

				while( !r.EndOfStream ) {
					string line = r.ReadLine();
					if(line == null ) {
						break;
					}
					line = line.Trim();
					if(line.Length == 0) {
						break;
					}
						
					if(line.StartsWith("msgid"))
					{
						if(e == null) e = new POEntry();
						if( e.msgid != null ) {
							Debug.LogWarning("[Bad entry]Skipping " + e.msgctxt + " " + e.msgid);
							break;
						}
						line = line.Replace("msgid", "").Trim().Trim('\"');
						e.msgid = line;
					}
					else if(line.StartsWith("msgctxt"))
					{
						if(e == null) e = new POEntry();
						if( e.msgctxt != null ) {
							Debug.LogWarning("[Bad entry]Skipping " + e.msgctxt + " " + e.msgid);
							break;
						}
						line = line.Replace("msgctxt", "").Trim().Trim('\"');
						e.msgctxt = line;
					}
					else if(line.StartsWith("msgstr"))
					{
						if(e == null) e = new POEntry();
						if( e.msgstr != null ) {
							Debug.LogWarning("[Bad entry]Skipping " + e.msgctxt + " " + e.msgid);
							break;
						}
						line = line.Replace("msgstr", "").Trim().Trim('\"');
						e.msgstr = line;
					}
					else if(line.StartsWith("\""))
					{
						if(e!=null && e.msgstr != null) {
							line = line.Trim().Trim('\"');
							e.msgstr = e.msgstr + line;
						}
					}
					else if(line.StartsWith("#:"))
					{
						if(e == null) e = new POEntry();
						line = line.Replace("#:", "").Trim();
						e.comments.Add(line);
					}
				}

				if(e!=null && e.ready) {
					return e;
				}

				return null;
			}

			public static void ImportFile(MultiLangStringDatabase db, string newPath, SystemLanguage refLanguage, bool updateRefLanguage)
			{
				List<POEntry> entries = new List<POEntry>();

				SystemLanguage parsedLanguage = SystemLanguage.Unknown;

				//start parsing each line
				using(StreamReader file = new StreamReader(newPath))
				{
					// read header
					parsedLanguage = ReadPOHeader(file);
					if( parsedLanguage == SystemLanguage.Unknown ) {
						Debug.LogError("Unknown language po file: " + newPath);
						return ;
					}

					while(!file.EndOfStream) {
						POEntry e = ReadNextEntry(file);
						if(e != null) {
							entries.Add(e);
						}
					}
				}

				///done parsing now add them to the language.asset file
				foreach(POEntry e in entries)
				{
					if( e.msgid == null || e.msgctxt == null || e.msgstr == null ) {
						Debug.Log("Skipping:" + e.msgctxt + " " + e.msgid);
						continue;
					}

					//add the key and value to the language file
					db.SetTextEntry(parsedLanguage, e.Key, e.msgstr, e.comments.ToArray());

					if( refLanguage != SystemLanguage.Unknown ) {
						string refValue = db[refLanguage].values[ db.IndexOfKey(e.Key) ].text;
						if( refValue != e.Key ) {
							if( updateRefLanguage ) {
								db.SetTextEntry(refLanguage, e.Key, e.msgid, e.comments.ToArray());
							} else {
								Debug.LogWarning("Reference Language is different for key["+e.msgctxt+"]:\\n Is: " + refValue +
									"\\nPO: " + e.msgid);
							}
						}
					}
				}

			}

			public static void ExportFile(MultiLangStringDatabase db, SystemLanguage targetLanguage, SystemLanguage referenceLanguage, string newPath)
			{

				StringBuilder builder = new StringBuilder();
				builder.Append(CreatePoHeaderEntry(targetLanguage)).AppendLine().AppendLine();
					
				//get the mLanguages Object
				//get the keys
				for(int i = 0; i < db.Count; ++i) 
				{
					//string key    = db[i];
					string msgid  = db[referenceLanguage].values[i].text;
					string msgstr = db[targetLanguage].values[i].text;
					string[] comments = db [targetLanguage].values [i].comments;

					//for each key, create an entry
					builder.Append( POUtility.CreatePOEntry(msgid, msgstr, comments) );
				}

				File.WriteAllText(newPath, builder.ToString());
			}
		}
	}
}
