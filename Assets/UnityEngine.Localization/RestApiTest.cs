using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using System.Text;
using System.IO;
using System;
using System.Security.Cryptography;

namespace UnityEngine.Localization
{
    public class RestApiTest
    {
        private static string authKey = "7d406b591205762ee83fdf93c354b83a6bc4f608";
        private static string serverUrl = "https://new-translate-api-staging.unity3d.jp";
        static List<string> listOfAllTransaltionFilesOnServer;

        // Works. https://new-translate-web-staging.unity3d.jp/api/user-api/get-user-info
        [MenuItem ("Tests/Rest/User Info")]
        public static void GetUserInfo()
        {
            Downloader.Response response;
            Downloader.SendGetData(serverUrl + "/v1/user/info", out response);
            Debug.Log(response);
        }

        // Works. https://new-translate-web-staging.unity3d.jp/api/project-api/get-project-list
        [MenuItem ("Tests/Rest/Get Projects List")]
        public static void GetProjectsList()
        {
            Downloader.Response response;
            Downloader.SendGetData(serverUrl + "/v1/projects?languageID=ru", out response);//?languageID=ru gets ignored?
            var projectsList = JsonUtility.FromJson<TranslateProjectList>(response.body);

            Debug.Log(projectsList.projects.Length);
        }

        // Works. https://new-translate-web-staging.unity3d.jp/api/project-api/get-project
        [MenuItem ("Tests/Rest/Get Project Info")]
        public static void GetProjectInfo()
        {
            Downloader.Response response;
            Downloader.SendGetData(serverUrl + "/v1/projects/" + TranslateProject.GetProjectDataForEditor().id, out response);

            var projectInfo = Json.Deserialize(response.body) as Dictionary<string, object>;

            foreach (KeyValuePair<string, object> entry in projectInfo)
                Debug.Log("Key: " + entry.Key + " Value: " + entry.Value);
        }

        // Works. https://new-translate-web-staging.unity3d.jp/api/project-api/get-file-list
        [MenuItem ("Tests/Rest/Get List of Files")]
        public static void GetListOfFiles()
        {
            Downloader.Response response;
            Downloader.SendGetData(serverUrl + "/v1/projects/1/topics/files?type=simple", out response);

            var listOfFiles = Json.Deserialize(response.body) as Dictionary<string, object>;
            listOfAllTransaltionFilesOnServer = ((List<object>)listOfFiles["files"]).ConvertAll(x => x.ToString());

            Debug.Log("GetListOfFiles; Number of files: " + listOfAllTransaltionFilesOnServer.Count);
        }

        // Works. For single file: https://new-translate-web-staging.unity3d.jp/api/file-api/get-translation-file
        // Downloading the file and converting it to Language-file format.
        [MenuItem ("Tests/Rest/Get Translation File")] //WORKS
        public static void GetTranslationFile()
        {
            var filePathOnServer = "Best Practices/1-1.physics_best_practices.md";
            var translationFile = DownloadTranslationFile(filePathOnServer);
            SaveTranslationFileToProjectLanguageFile(translationFile);
        }

        // Doesn't work. Need to upload source file first in PO format and then
        // upload actual translation file, as well in PO format. 
        // Currenty getting errors from the server while trying to upload source file.
        [MenuItem ("Tests/Rest/Upload Translation File")]
        public static void UploadTranslationFile()
        {
            var localFilePath = "Best Practices/2-1.a_guide_to_assetbundles_and_resources.asset";
            if (!IsSourceFileOnServerAndIsNotModified(localFilePath))
                UploadSourceFile(localFilePath);
            UploadTranslationFile(localFilePath, SystemLanguage.Japanese); // TODO: Get SystemLanguage for current exporting language 
        }

        //Should check if the file is there or if it is of the same version/condition. 
        static bool IsSourceFileOnServerAndIsNotModified(string localFilePath)
        {
            string serverFilePath = GetServerPathForLocalPath(localFilePath);

            var absoluteFilePath = Path.Combine(Application.dataPath, localFilePath);
            
            var md5 = GetMD5HashForFile(absoluteFilePath); // TODO: Fix it. I need hash for converted POT file and not for the LANG file.

            var requestURL = serverUrl + "/v1/files/source-file/actions/verify?projectID="
                             + TranslateProject.GetProjectDataForEditor().id
                             + "&branch=" + TranslateProject.GetProjectDataForEditor().currentBranch
                             + "&file=" + serverFilePath + "&md5=" + md5;
            Debug.Log("IsSourceFileOnServerAndIsNotModified\n" + requestURL);
            Downloader.Response response;
            var success = Downloader.SendGetData(requestURL, out response);
            Debug.Log("IsSourceFileOnServerAndIsNotModified:\n" + response);
            if (success && response.body.Contains("true"))
                return true;
            return false;
        }

        static string GetServerPathForLocalPath(string localFilePath)
        {
            // https://new-translate-api-staging.unity3d.jp/v1/files/source-file/actions/verify?projectID=1&branch=topics&file=Best%20Practices%2F1-1.physics_best_practices.md&md5=dd9391cd42e5bf63888208e2b835172c
            return Path.ChangeExtension(localFilePath, ".pot");
        }

        static string GetMD5HashForFile(string absoluteFilePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(absoluteFilePath))
                {
                    var md5hash = md5.ComputeHash(stream);
                    var result = new StringBuilder(md5hash.Length * 2);
                    for (int i = 0; i < md5hash.Length; i++)
                        result.Append(md5hash[i].ToString("x2"));
                    return result.ToString();
                }
            }
        }

        // https://new-translate-web-staging.unity3d.jp/api/file-api/upload-translation-file
        private static void UploadTranslationFile(string localFilePath, SystemLanguage targetLanguage)
        {
            var uploadFile = new Dictionary<string, object>();
            var translationFilePath = Path.ChangeExtension(localFilePath, ".po");
            uploadFile["name"] = translationFilePath;
            uploadFile["mimeType"] = "application/x-po";
            var fileContent = GetFileDataInPoFormat(localFilePath, targetLanguage);
            uploadFile["text"] = fileContent;

            var data = new Dictionary<string, object>();
            data["projectID"] = TranslateProject.GetProjectDataForEditor().id;
            data["branch"] = TranslateProject.GetProjectDataForEditor().currentBranch;
            data["languageID"] = "ja";//targetLanguage.ToString(); // TODO: can't use SystemLanguage here. Need either "13" or "ja" for Japanese. Need conversion between SystemLanguage and the one that is supported on the server.
            var sourceFilePath = GetServerPathForLocalPath(localFilePath); // Source file name on server. Saving it with 'pot' extension.
            data["filename"] = sourceFilePath;
            data["uploadFile"] = uploadFile;

            var jsonData = Json.Serialize(data);
            Debug.Log("UploadTranslationFile jsonData:\n" + jsonData);
            Downloader.Response response;
            Downloader.SendGetPostData(serverUrl + "/v1/files/translation-file"
                                        , jsonData, out response);
            Debug.Log("UploadTranslationFile:\n" + response);
        }

        // FAILS. https://new-translate-web-staging.unity3d.jp/api/file-api/upload-source-file
        // TODO: Need a way to extract source file in PO format without translations.
        // TODO: References (Starts With "#:") in LANG file need be 'per-key' and not 'per-language'
        private static void UploadSourceFile(string localFilePath)
        {
            var fileEntry = new Dictionary<string, object>();
            var remoteFilePath = GetServerPathForLocalPath(localFilePath);
            fileEntry["name"] = remoteFilePath;
            fileEntry["mimeType"] = "application/x-po";

            var fileContent = GetFileDataInPoFormat(localFilePath,
                                SystemLanguage.Japanese);
            fileEntry["text"] = fileContent;

            var filesToUpload = new List<object>();
            filesToUpload.Add(fileEntry);

            var data = new Dictionary<string, object>();
            data["uploadFiles"] = filesToUpload;

            var jsonData = Json.Serialize(data);
            // Debug.Log("UploadSourceFile jsonData");
            // Debug.Log(jsonData);
            Downloader.Response response;
            Downloader.SendGetPostData(serverUrl
                            + "/v1/projects/" + TranslateProject.GetProjectDataForEditor().id
                            + "/" + TranslateProject.GetProjectDataForEditor().currentBranch
                            + "/files"
                            , jsonData
                            , out response);
            Debug.Log("UploadSourceFile\n" + response);
        }

        // Appears to work OK
        private static string GetFileDataInPoFormat(string localFilePath, SystemLanguage targetLanguage)
        {
            var assetPath = Path.Combine("Assets", localFilePath);
            var db = AssetDatabase.LoadAssetAtPath<MultiLangStringDatabase>(assetPath);
            if (db == null)
            {
                throw new Exception("didn't load asset");
            }
            var tempPath = Path.GetTempFileName();
            POUtility.ExportFile(db, targetLanguage,
                        (SystemLanguage)TranslateProject.GetProjectDataForEditor().sourceLanguageID, tempPath);

            var convertedContent = File.ReadAllText(tempPath);
            File.Delete(tempPath);

            Debug.Log("GetFileDataInPoFormat > targetLang: " + targetLanguage);
            Debug.Log("GetFileDataInPoFormat > convertedContent:\n" + convertedContent);

            return convertedContent;
        }

        // Works. Same as GetTranslationFile() but for all files in the list. 
        // There's an option here (https://new-translate-web-staging.unity3d.jp/api/file-api/get-translated-file)
        // to download all files in one batch, but it was failing at the moment of implementation.
        [MenuItem ("Tests/Rest/Get All Translation Files")]
        static void GetAllTranslationFiles()
        {
            if (listOfAllTransaltionFilesOnServer == null || listOfAllTransaltionFilesOnServer.Count == 0)
                GetListOfFiles();

            foreach (var translationFilePath in listOfAllTransaltionFilesOnServer)
            {
                var translationFile = DownloadTranslationFile(translationFilePath);
                SaveTranslationFileToProjectLanguageFile(translationFile);
            }
        }

        // Works. Takes file name as it is on server. Downloads it. Returns as TranslationFile.
        static TranslationFile DownloadTranslationFile(string filePathOnServer)
        {
            var translationFile = new TranslationFile();

            translationFile.pathOnServer = filePathOnServer;
            filePathOnServer = WWW.EscapeURL(filePathOnServer);

            Downloader.Response response;
            Downloader.SendGetData(serverUrl
                            + "/v1/files/translation-file?projectID=1&languageID=ja&branch=topics&file="
                            + filePathOnServer,
                            out response);
            translationFile.content = response.body;

            return translationFile;
        }

        // Works.
        // Gets TranslationFile. Converts it to 'Language' file format. Saves it in Assets project
        // folder in the same relative path as was on the server.
        private static void SaveTranslationFileToProjectLanguageFile(TranslationFile translationFile)
        {
            var tempPath = Path.GetTempFileName();
            File.WriteAllText(tempPath, translationFile.content);

            var db = ScriptableObject.CreateInstance<MultiLangStringDatabase>();
            POUtility.ImportFile(db, tempPath, SystemLanguage.English, true); // TODO: Update language from settings
            File.Delete(tempPath);

            var filePath = Path.Combine(Application.dataPath, translationFile.pathOnServer);
            (new FileInfo(filePath)).Directory.Create();
            AssetDatabase.Refresh();

            filePath = Path.Combine("Assets", translationFile.pathOnServer);
            filePath = Path.ChangeExtension(filePath, ".asset");
            AssetDatabase.CreateAsset(db, filePath);
        }

        // NOT COMPLETE IMPLEMENTATION. https://new-translate-web-staging.unity3d.jp/api/project-api/create-branch
        // [MenuItem ("Tests/Rest/Create Branch")] 
        // public static void CreateBranch()
        // {
        // var branch = new Branch();
        // branch.branch = "MyNewBranchName";
        // var data = JsonUtility.ToJson(branch);
        // www = new UnityWebRequest(serverUrl + "/v1/projects/17/branch", "POST");
        // byte[] bodyRaw = Encoding.UTF8.GetBytes(data);
        // www.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);
        // www.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
        // www.SetRequestHeader("Content-Type", "application/json");
        // www.SetRequestHeader("AUTHORIZATION", "Basic " + authKey);
        // www.SendWebRequest();
        // EditorApplication.update += EditorUpdate;
        // }

        // Works. https://new-translate-web-staging.unity3d.jp/api/project-api/create-project
        // [MenuItem ("Tests/Rest/Create Project")] 
        // public static void CreateProject()
        // {
        //     var newProject = new TranslateProject();
        //     newProject.name = "Not another project!";
        //     newProject.organizationID = "7971460616196";
        //     newProject.sourceLanguageID = 6;
        //     newProject.targetLanguageIDs = new int[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20};
        //     newProject.repositoryHosting = 0;
        //     var data = JsonUtility.ToJson(newProject);

        //     Downloader.SendGetPostData(serverUrl + "/v1/projects", data);
        // }

        class Downloader
        {
            public struct Response
            {
                public string body;
                public long code;
                public string error;
                public override string ToString()
                {
                    var response = String.Format("Response code: {0}\nResponse error message: {1}\nResponse body:\n{2}", code, error, body);
                    return response;
                }
            }
            
            UnityWebRequest request;
            Response response;
            bool IsRequestSeccessfull;

            public Downloader(UnityWebRequest request)
            {
                this.request = request;
            }

            // For GET requests to get and update data.
            // Returns TRUE if all is well. False otherwise.
            public static bool SendGetData(string url, out Response response)
            {
                var request = UnityWebRequest.Get(url);
                var downloader = new Downloader(request);
                return downloader.GetData(out response);
            }

            // For POST requests to get and update data.
            public static bool SendGetPostData(string url, string data, out Response response)
            {
                var request = new UnityWebRequest(url, "POST");
                byte[] bodyRaw = Encoding.UTF8.GetBytes(data);
                request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                var downloader = new Downloader(request);
                return downloader.GetData(out response);
            }

            // This method is syncrhonous and locks the Editor.
            public bool GetData(out Response response)
            {
                IEnumerator e = Request();
                while (e.MoveNext())
                    if (e.Current != null)
                        Debug.Log(e.Current as string);

                response = this.response;
                return IsRequestSeccessfull;
            }

            IEnumerator Request()
            {
                request.SetRequestHeader("AUTHORIZATION", "Basic " + authKey);
                request.SetRequestHeader("Client-ID", "unity");
                request.SendWebRequest();

                while (!request.isDone)
                    yield return null;

                response.body = request.downloadHandler.text;
                response.code = request.responseCode;
                response.error = request.error;

                if (request.isHttpError || request.isNetworkError)
                {
                    IsRequestSeccessfull = false;
                    Debug.Log("Got error during server query:\n" + response);
                }
                else
                {
                    IsRequestSeccessfull = true;
                }
            }
        }
    }

    // TODO: Might want to clean up the structure or get rid of it altogether, 
    // replacing JsonUtility implementation with Json.
    [System.Serializable]
    class TranslateProject
    {
        public bool deleted = false;
        public int id = -1;
        public string name = null;
        public string description = null;
        public string organizationID = null;
        public int sourceLanguageID = -1;
        public int[] targetLanguageIDs = null;
        public int repositoryHosting = 0;
        public string[] branches = null;
        public string icon = null;
        public string currentBranch;

        public static TranslateProject GetProjectDataForEditor()
        {
            var project = new TranslateProject();
            project.id = 23;
            project.currentBranch = "master";
            project.sourceLanguageID = (int)SystemLanguage.English;
            return project;
        }
    }

    [System.Serializable]
    class TranslateProjectList
    {
        public TranslateProject[] projects = null;
    }

    struct TranslationFile
    {
        public string pathOnServer;
        public string content;
    }
}