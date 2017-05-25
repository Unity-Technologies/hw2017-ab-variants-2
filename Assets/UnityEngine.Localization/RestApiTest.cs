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

        // TODO: Need a way to do the mapping dynamically. 
        static Dictionary<string, string> serverLanguageCodes = new Dictionary<string, string>
        { // Add here whatever languages are supported on the server.
            {"Arabic", "ar"},
            {"Czech", "cs"},
            {"Danish", "da"},
            {"German", "de"},
            {"Greek", "el"},
            {"English", "en"},
            {"Spanish", "es"},
            {"Finnish", "fi"},
            {"French", "fr"},
            {"Hebrew", "he"},
            {"Hungarian", "hu"},
            {"Italian", "it"},
            {"Japanese", "ja"},
            {"Korean", "ko"},
            {"Polish", "pl"},
            {"Portuguese", "pt"},
            {"Dutch", "nl"},
            {"Slovak", "sk"},
            {"Slovenian", "sl"},
            {"Swedish", "sv"},
            {"Turkish", "tr"},
            {"Russian", "ru"}
        };

        // // Works. https://new-translate-web-staging.unity3d.jp/api/user-api/get-user-info
        // [MenuItem ("Tests/Rest/User Info")]
        // public static void GetUserInfoTest()
        // {
        //     Downloader.Response response;
        //     Downloader.SendGetData(serverUrl + "/v1/user/info", out response);
        //     Debug.Log(response);
        // }

        // // Works. https://new-translate-web-staging.unity3d.jp/api/project-api/get-project-list
        // [MenuItem ("Tests/Rest/Get Projects List")]
        // public static void GetProjectsListTest()
        // {
        //     Downloader.Response response;
        //     Downloader.SendGetData(serverUrl + "/v1/projects?languageID=ru", out response);//?languageID=ru gets ignored?
        //     var projectsList = JsonUtility.FromJson<TranslateProjectList>(response.body);

        //     Debug.Log(projectsList.projects.Length);
        // }

        // // Works. https://new-translate-web-staging.unity3d.jp/api/project-api/get-project
        // [MenuItem ("Tests/Rest/Get Project Info")]
        // public static void GetProjectInfoTest()
        // {
        //     Downloader.Response response;
        //     Downloader.SendGetData(serverUrl + "/v1/projects/" + TranslateProject.GetProjectDataForEditor().id, out response);

        //     var projectInfo = Json.Deserialize(response.body) as Dictionary<string, object>;

        //     foreach (KeyValuePair<string, object> entry in projectInfo)
        //         Debug.Log("Key: " + entry.Key + " Value: " + entry.Value);
        // }

        [MenuItem ("Tests/Rest/Get List of Files")]
        static void GetListOfFilesTest()
        {
            listOfAllTransaltionFilesOnServer = GetListOfFiles(new TranslateProject());
            Debug.Log("GetListOfFiles; Number of files: " + listOfAllTransaltionFilesOnServer.Count);
        }

        // Works. https://new-translate-web-staging.unity3d.jp/api/project-api/get-file-list
        public static List<string> GetListOfFiles(TranslateProject projectSettings)
        {
            Downloader.Response response;
            Downloader.SendGetData(serverUrl
                                    + "/v1/projects/" + projectSettings.id
                                    + "/" + projectSettings.currentBranch
                                    + "/files?type=simple"
                                    , out response);

            var listOfFiles = Json.Deserialize(response.body) as Dictionary<string, object>;
            return ((List<object>)listOfFiles["files"]).ConvertAll(x => x.ToString());
        }
        
        [MenuItem ("Tests/Rest/Get Translation File")]
        static void GetTranslationFileTest()
        {
            var filePathOnServer = "Best Practices/1-1.physics_best_practices.pot"; // TODO: Get this from API.
            GetTranslationFile(filePathOnServer, SystemLanguage.Russian, new TranslateProject());
        }

        // Works. For single file: https://new-translate-web-staging.unity3d.jp/api/file-api/get-translation-file
        // Downloading the file and converting it to Language-file format.
        public static void GetTranslationFile(string filePathOnServer, SystemLanguage translationLanguage, TranslateProject projectSettings)
        {
            var translationFile = DownloadTranslationFile(filePathOnServer, translationLanguage, projectSettings);
            SaveTranslationFileToProjectLanguageFile(translationFile, projectSettings);
        }

        // Works.
        [MenuItem ("Tests/Rest/Upload Translation File")]
        static void UploadTranslationFileTest()
        {
            var localFilePath = "Best Practices/1-1.physics_best_practices.asset";
            UploadTranslationFile(localFilePath, SystemLanguage.Russian, new TranslateProject());
        }

        public static void UploadTranslationFile(string localFilePath, SystemLanguage translationLanguage, TranslateProject projectSettings)
        {
            if (!IsSourceFileOnServerAndIsNotModified(localFilePath, projectSettings))
                UploadSourceFile(localFilePath, projectSettings);
            SendTranslationFileToServer(localFilePath, translationLanguage, projectSettings);
        }

        //Should check if the file is there or if it is of the same version/condition. 
        // https://new-translate-api-staging.unity3d.jp/v1/files/source-file/actions/verify?projectID=1&branch=topics&file=Best%20Practices%2F1-1.physics_best_practices.md&md5=dd9391cd42e5bf63888208e2b835172c
        private static bool IsSourceFileOnServerAndIsNotModified(string localFilePath, TranslateProject projectSettings)
        {
            string serverFilePath = GetServerPathForLocalPath(localFilePath);
            var absoluteFilePath = GetAbsoluteFilePath(localFilePath);
            var md5 = GetMD5HashForFileContent(absoluteFilePath); // TODO: Fix it. Hash for converted POT file and not for the LANG DB file.
            var requestURL = serverUrl + "/v1/files/source-file/actions/verify?"
                                + "projectID=" + projectSettings.id
                                + "&branch=" + projectSettings.currentBranch
                                + "&file=" + serverFilePath
                                + "&md5=" + md5;
            Downloader.Response response;
            var success = Downloader.SendGetData(requestURL, out response);
            if (success && response.body.Contains("true"))
                return true;
            return false;
        }

        // Source file name on server. Using 'pot' extension.
        private static string GetServerPathForLocalPath(string localFilePath)
        {
            return Path.ChangeExtension(localFilePath, ".pot");
        }

        private static string GetMD5HashForFileContent(string absoluteFilePath)
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
        private static void SendTranslationFileToServer(string localFilePath, SystemLanguage targetLanguage, TranslateProject projectSettings)
        {
            var uploadFile = new Dictionary<string, object>();
            var translationFilePath = Path.ChangeExtension(localFilePath, ".po");
            uploadFile["name"] = translationFilePath;
            uploadFile["mimeType"] = "application/x-po";
            var fileContent = GetFileDataInPoFormat(localFilePath, targetLanguage, projectSettings);
            uploadFile["text"] = fileContent;

            var data = new Dictionary<string, object>();
            data["projectID"] = projectSettings.id;
            data["branch"] = projectSettings.currentBranch;
            data["languageID"] = serverLanguageCodes[targetLanguage.ToString()]; // Can't use SystemLanguage here. Need conversion between SystemLanguage and the one that is supported on the server.
            var sourceFilePath = GetServerPathForLocalPath(localFilePath); 
            data["filename"] = sourceFilePath;
            data["uploadFile"] = uploadFile;

            var jsonData = Json.Serialize(data);
            Downloader.Response response;
            Downloader.SendGetPostData(serverUrl + "/v1/files/translation-file"
                                        , jsonData, out response);
        }

        // Works. https://new-translate-web-staging.unity3d.jp/api/file-api/upload-source-file
        // TODO: Need a way to extract source file in PO format without translations.
        // TODO: References (Starts With "#:") in LANG file need be 'per-key' and not 'per-language'
        private static void UploadSourceFile(string localFilePath, TranslateProject projectSettings)
        {
            var fileEntry = new Dictionary<string, object>();
            var remoteFilePath = GetServerPathForLocalPath(localFilePath);
            fileEntry["name"] = remoteFilePath;
            fileEntry["mimeType"] = "application/x-po";

            var fileContent = GetFileDataInPoFormat(localFilePath,
                                SystemLanguage.Japanese, projectSettings); // TODO: FixIt ^^^
            fileEntry["text"] = fileContent;

            var filesToUpload = new List<object>();
            filesToUpload.Add(fileEntry);

            var data = new Dictionary<string, object>();
            data["uploadFiles"] = filesToUpload;

            var jsonData = Json.Serialize(data);
            Downloader.Response response;
            Downloader.SendGetPostData(serverUrl
                            + "/v1/projects/" + projectSettings.id
                            + "/" + projectSettings.currentBranch
                            + "/files"
                            , jsonData
                            , out response);
            // Debug.Log("UploadSourceFile\n" + response);
        }

        // Appears to work OK
        private static string GetFileDataInPoFormat(string localFilePath, SystemLanguage targetLanguage, TranslateProject projectSettings)
        {
            var db = GetMultiLangStringDB(localFilePath);
            var tempPath = Path.GetTempFileName();
            POUtility.ExportFile(db, targetLanguage,
                        (SystemLanguage)projectSettings.sourceLanguageID, tempPath);

            var convertedContent = File.ReadAllText(tempPath);
            File.Delete(tempPath);

            // Debug.Log("GetFileDataInPoFormat > targetLang: " + targetLanguage);
            // Debug.Log("GetFileDataInPoFormat > convertedContent:\n" + convertedContent);

            return convertedContent;
        }

        // Works. Same as GetTranslationFile() but for all files in the list. 
        // There's an option here (https://new-translate-web-staging.unity3d.jp/api/file-api/get-translated-file)
        // to download all files in one batch, but it was failing at the moment of implementation.
        [MenuItem ("Tests/Rest/Get All Translation Files")]
        static void GetAllTranslationFilesTest()
        {
            DownloadAndSaveAllTranslationFiles(SystemLanguage.Japanese, new TranslateProject());
        }

        public static void DownloadAndSaveAllTranslationFiles(SystemLanguage translationLanguage, TranslateProject projectSettings)
        {
            var listOfAllTransaltionFilesOnServer = GetListOfFiles(projectSettings);

            foreach (var translationFilePath in listOfAllTransaltionFilesOnServer)
            {
                var translationFile = DownloadTranslationFile(translationFilePath,
                                             translationLanguage, projectSettings);
                SaveTranslationFileToProjectLanguageFile(translationFile, projectSettings);
            }
        }

        // Works. Takes file name as it is on server. Downloads it. Returns as TranslationFile.
        private static TranslationFile DownloadTranslationFile(string filePathOnServer, SystemLanguage translationLanguage, TranslateProject projectSettings)
        {
            var translationFile = new TranslationFile();

            translationFile.pathOnServer = filePathOnServer;
            filePathOnServer = WWW.EscapeURL(filePathOnServer);

            Downloader.Response response;
            Downloader.SendGetData(serverUrl // TODO: Get values here from API.
                        + "/v1/files/translation-file?projectID=" + projectSettings.id
                        + "&languageID=" + serverLanguageCodes[translationLanguage.ToString()]
                        + "&branch=" + projectSettings.currentBranch 
                        + "&file=" + filePathOnServer,
                        out response);
            translationFile.content = response.body;

            return translationFile;
        }

        // Works.
        // Gets TranslationFile. Converts it to 'Language' file format. Saves it in Assets project
        // folder in the same relative path as was on the server.
        private static void SaveTranslationFileToProjectLanguageFile(TranslationFile translationFile, TranslateProject projectSettings)
        {
            var tempPath = Path.GetTempFileName();
            File.WriteAllText(tempPath, translationFile.content);

            var db = GetMultiLangStringDB(translationFile.pathOnServer);
            POUtility.ImportFile(db, tempPath, (SystemLanguage)projectSettings.sourceLanguageID, true); // TODO: 1. Update reference language from settings. 2. Q.: When last param true and when false?
            File.Delete(tempPath);
            
            SaveLangDBFileAtPath(db, translationFile.pathOnServer);
        }

        private static void SaveLangDBFileAtPath(MultiLangStringDatabase db, string storeFilePath)
        {
            storeFilePath = Path.ChangeExtension(storeFilePath, ".asset");
            var absoluteFilePath = GetAbsoluteFilePath(storeFilePath);
            storeFilePath = Path.Combine("Assets", storeFilePath);
            if (File.Exists(absoluteFilePath)) 
            { // update existing asset
                var existingDb = AssetDatabase.LoadMainAssetAtPath(storeFilePath) as MultiLangStringDatabase;
                EditorUtility.CopySerialized(db, existingDb);
                AssetDatabase.SaveAssets();
            }
            else
            { // create new asset
                (new FileInfo(absoluteFilePath)).Directory.Create();
                AssetDatabase.Refresh();
                AssetDatabase.CreateAsset(db, storeFilePath);
            }
        }

        // Loads existing LANG file if there's one, or generates new instance if there's none. 
        private static MultiLangStringDatabase GetMultiLangStringDB(string localDbFilePath)
        {
            localDbFilePath = Path.ChangeExtension(localDbFilePath, ".asset");
            var absoluteFilePath = GetAbsoluteFilePath(localDbFilePath);
            if (File.Exists(absoluteFilePath))
            {
                var assetPath = Path.Combine("Assets", localDbFilePath);
                return AssetDatabase.LoadAssetAtPath<MultiLangStringDatabase>(assetPath);
            }
            return ScriptableObject.CreateInstance<MultiLangStringDatabase>();
        }

        private static string GetAbsoluteFilePath(string localFilePath)
        {
            return Path.Combine(Application.dataPath, localFilePath);
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
    public class TranslateProject
    {
        public TranslateProject(int id = 23, string currentBranch = "master", SystemLanguage projectLanguage = SystemLanguage.English)
        {
            this.id = id;
            this.currentBranch = currentBranch;
            this.sourceLanguageID = (int)projectLanguage;
        }
        
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
            return project;
        }
    }

    [System.Serializable]
    class TranslateProjectList
    {
        public TranslateProject[] projects = null;
    }

    public struct TranslationFile
    {
        public string pathOnServer;
        public string content;
    }
}