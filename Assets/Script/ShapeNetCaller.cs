using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Cysharp.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace ModelPlacement
{
    public struct ModelResourcePath
    {
        public string objPath;
        public string mtlPath;
        public List<string> texturePaths;
        
        public ModelResourcePath(string objPath, string mtlPath, List<string> texturePaths)
        {
            this.objPath = objPath;
            this.mtlPath = mtlPath;
            this.texturePaths = texturePaths;
        }
    }
    public class ShapeNetCaller
    {
        private string _baseUrl;
        private string _downloadDirectory;
        private string _relativeDownloadDirectory = "Cache";
        private JArray _metaAllJson;

        public string DownloadDirectory
        {
            get { return _downloadDirectory; }
        }

        public ShapeNetCaller(string baseUrl)
        {
            _baseUrl = baseUrl;
            _downloadDirectory = Path.Combine(Application.persistentDataPath, _relativeDownloadDirectory);
            if (!Directory.Exists(_downloadDirectory))
            {
                Directory.CreateDirectory(_downloadDirectory);
            }
        }

        public async UniTask DownloadModel(ModelData modelData, KeyValuePair<JObject,JObject> modelMeta,
            UnityAction<ModelData, KeyValuePair<JObject,JObject>, ModelResourcePath> OnCompleted)
        {
            // APIへのリクエスト
            string apiUrl = $"{_baseUrl}/model/meta/{modelData.model_id}/{modelData.model_child_id}";
            UnityWebRequest request = UnityWebRequest.Get(apiUrl);
            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + request.error);
                return;
            }

            ModelApiResponse modelApiResponse = JsonUtility.FromJson<ModelApiResponse>(request.downloadHandler.text);

            // ダウンロード先ディレクトリの作成
            // TODO: modelData.model_idをresponseの方でも対応するか考える
            string resourceDir = Path.Combine(_downloadDirectory, modelData.model_id + "_" + modelApiResponse.model_child_id);
            Directory.CreateDirectory(resourceDir);
            string modelDir = Path.Combine(resourceDir, "models");
            string textureDir = Path.Combine(resourceDir, "images");
            Directory.CreateDirectory(modelDir);
            Directory.CreateDirectory(textureDir);

            // objファイルのダウンロード
            string objUrl = _baseUrl + "/" + modelApiResponse.model_path.obj;
            string objPath = Path.Combine(modelDir, Path.GetFileName(objUrl));
            if (File.Exists(objPath) == false)
            {
                await DownloadFile(objUrl, objPath);
            }

            // mtlファイルのダウンロード
            string mtlUrl = _baseUrl + "/" + modelApiResponse.model_path.mtl;
            string mtlPath = Path.Combine(modelDir, Path.GetFileName(mtlUrl));
            if (File.Exists(mtlPath) == false)
            {
                await DownloadFile(mtlUrl, mtlPath);
            }

            // テクスチャファイルのダウンロード
            List<string> texturePaths = new List<string>();
            foreach (string textureUrl in modelApiResponse.texture_paths)
            {
                string fullTextureUrl = _baseUrl + "/" + textureUrl;
                string texturePath = Path.Combine(textureDir, Path.GetFileName(textureUrl));
                texturePaths.Add(texturePath);
                if (File.Exists(texturePath) == false)
                {
                    await DownloadFile(fullTextureUrl, texturePath);
                }
            }
            ModelResourcePath modelResourcePath = new ModelResourcePath(objPath, mtlPath, texturePaths);

            OnCompleted(modelData, modelMeta, modelResourcePath);
        }

        public async UniTask<string> GetModelMetaForChatGPT()
        {
            string url = $"{_baseUrl}/model/meta_all/for_chat_gpt";
            UnityWebRequest request = UnityWebRequest.Get(url);
            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var text = request.downloadHandler.text;
                JArray meta = JArray.Parse(text);
                Debug.LogFormat("GetModelMetaForChatGPT: {0}, tex: {1}", meta.ToString(), text);
                //Debug.LogFormat("tex: {0}",text);

                //OnCompleted(text);
                return text;
            }
            else
            {
                Debug.LogError("Error: " + request.error);
                return null;
            }
        }

        public async UniTask<JArray> GetModelMetaAll()
        {
            string url = $"{_baseUrl}/model/meta_all";
            UnityWebRequest request = UnityWebRequest.Get(url);
            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var text = request.downloadHandler.text;
                var metaAllJson = JArray.Parse(text);
                //Debug.LogFormat("GetModelMetaForChatGPT: {0}, tex: {1}", meta.ToString(), text);
                //Debug.LogFormat("tex: {0}",text);

                //OnCompleted(text);
                return metaAllJson;
            }
            else
            {
                Debug.LogError("Error: " + request.error);
                return null;
            }
        }

        private async UniTask DownloadFile(string url, string filePath)
        {
            UnityWebRequest fileRequest = UnityWebRequest.Get(url);
            await fileRequest.SendWebRequest();

            if (fileRequest.result == UnityWebRequest.Result.ConnectionError || fileRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error downloading file: " + url + " | Error: " + fileRequest.error);
                return;
            }
            File.WriteAllBytes(filePath, fileRequest.downloadHandler.data);
            Debug.Log("File downloaded and saved: " + filePath);
        }

        public async UniTask<bool> UpdateSeedApiRequest(int seed)
        {
            string apiUrl = $"{_baseUrl}/setting/seed/{seed}";
            using (UnityWebRequest request = UnityWebRequest.Put(apiUrl, ""))
            {
                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("Update seed request successfully sent");
                    return true; //correct
                }
                else
                {
                    Debug.LogError($"Error: {request.error}");
                    return false;
                }
            }
        }

        public void DeleteCache()
        {
            this.DeleteDirectoryContents(_downloadDirectory);
        }

        void DeleteDirectoryContents(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);

                // Delete all files in the directory
                foreach (FileInfo file in directoryInfo.GetFiles())
                {
                    file.Delete();
                }

                // Delete all subdirectories in the directory
                foreach (DirectoryInfo subDirectory in directoryInfo.GetDirectories())
                {
                    subDirectory.Delete(true); // 'true' parameter deletes recursively
                }
            }
            else
            {
                Debug.LogWarning("Directory not found: " + directoryPath);
            }
        }

    }
}
