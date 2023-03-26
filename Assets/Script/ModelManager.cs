using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Dummiesman;
using Cysharp.Threading.Tasks;
using System.IO;

namespace ModelPlacement
{
    public class ModelManager : MonoBehaviour
    {
        [SerializeField] bool isDebug = false;

        private const string BASE_URL = "http://127.0.0.1:8000/api";
        private const string DOWNLOADED_FILE_DIR = "AppData";
        private ModelTransformGetter manager;
        private ShapeNetCaller shapeNetCaller;

        public ShapeNetCaller ShapeNetCaller 
        {
            get { return shapeNetCaller; }
        }

        void Start()
        {
            manager = new ModelTransformGetter();
            shapeNetCaller = new ShapeNetCaller(BASE_URL);

            UpdateModelAndPosition();
        }

        public async void PlaceModel(string gptResult, JArray modelMetaAll)
        {
            ModelDataList modelDataList = JsonUtility.FromJson<ModelDataList>(gptResult);
            List<UniTask> downloadTasks = new List<UniTask>();

            foreach (ModelData modelData in modelDataList.models)
            {
                KeyValuePair<JObject,JObject> modelMeta = FindModelByIds(modelMetaAll, modelData.model_id, modelData.model_child_id);
                if (modelMeta.Key == null || modelMeta.Value == null) {
                    Debug.LogErrorFormat(
                        "ModelMeta was not founed in modelMetaAll from ShapeNetAPI: model_id{0}, model_child_id: {1}",
                        modelData.model_id, modelData.model_child_id);
                }
                downloadTasks.Add(shapeNetCaller.DownloadModel(modelData, modelMeta, OnCompleteDownloading));
            }

            await UniTask.WhenAll(downloadTasks);
            Debug.Log("Finish!");
        }

        public KeyValuePair<JObject,JObject> FindModelByIds(JArray modelsArray, string modelIdToFind, string modelChildIdToFind)
        {
            KeyValuePair<JObject,JObject> pair = new KeyValuePair<JObject,JObject>();

            foreach (JObject model in modelsArray)
            {
                string modelId = model["model_id"].ToString();

                if (modelId == modelIdToFind)
                {
                    JArray metaArray = (JArray)model["meta"];

                    foreach (JObject meta in metaArray)
                    {
                        string modelChildId = meta["model_child_id"].ToString();

                        if (modelChildId == modelChildIdToFind)
                        {
                            pair = new KeyValuePair<JObject, JObject>(model, meta);
                            return pair;
                        }
                    }
                }
            }

            return pair;
        }


        public void AskAndPlaceModel(string prompt, JArray modelMetaAll)
        {
            string modelTransform;
            if (isDebug) {
                TextAsset textAsset = Resources.Load<TextAsset>("TestJson"); //for test
                modelTransform = textAsset.text;
            }
            else
            {
                modelTransform = manager.GetTransformJson(prompt); //heavy
                WriteToFile("Assets/Resources/TestJson.json", modelTransform);
            }
            if (modelTransform == "")
            {
                Debug.LogError("transform json is empty.");
                return;
            }
            Debug.Log("ChatGPT result: " + modelTransform);
            PlaceModel(modelTransform, modelMetaAll);
        }



        private void WriteToFile(string filePath, string content)
        {
            try
            {
                File.WriteAllText(filePath, content);
                Debug.Log("テキストファイルに書き込みました。");
            }
            catch (System.Exception e)
            {
                Debug.LogError("書き込みエラー: " + e.Message);
            }
        }



        private void OnCompleteDownloading(ModelData modelData, KeyValuePair<JObject,JObject> modelMeta, ModelResourcePath modelResourcePath)
        {
            // 3Dモデルの生成
            GameObject model = new OBJLoader().Load(modelResourcePath.objPath, modelResourcePath.mtlPath);
            //model.name = modeldata.model_name;
            model.name = modelMeta.Key["model_name"].ToString();

            model.transform.position = new Vector3(
                modelData.transform.position[0],
                modelData.transform.position[1],
                modelData.transform.position[2]
            );
            model.transform.rotation = Quaternion.Euler(
                modelData.transform.rotation[0],
                modelData.transform.rotation[1],
                modelData.transform.rotation[2]
            );
            float scale = (float)modelMeta.Value["model_scale"];
            Debug.Log("model scale: " + scale);
            model.transform.localScale = Vector3.one * scale;
            
            model.transform.parent = this.transform;
        }

        public async void UpdateModelAndPosition()
        {
            DeleteAllChildren(this.gameObject);

            int seed;
            if (isDebug)
            {
                seed = PlayerPrefs.GetInt("seed");
            }
            else
            {
                seed = Random.Range(0, 10000);
                PlayerPrefs.SetInt("seed", seed);
            }
            bool success = await shapeNetCaller.UpdateSeedApiRequest(seed);
            if (!success) { return; }
            var task1 = shapeNetCaller.GetModelMetaForChatGPT();
            var task2 = shapeNetCaller.GetModelMetaAll();
            var (prompt, modelMetaAll) = await UniTask.WhenAll(task1, task2);

            Debug.Log("prompt: " + prompt);
            Debug.Log("modelMetaAll: " + modelMetaAll);

            AskAndPlaceModel(prompt, modelMetaAll);
        }

        public async void UpdatePosition()
        {
            DeleteAllChildren(this.gameObject);

            var task1 = shapeNetCaller.GetModelMetaForChatGPT();
            var task2 = shapeNetCaller.GetModelMetaAll();
            var (prompt, modelMetaAll) = await UniTask.WhenAll(task1, task2);
            AskAndPlaceModel(prompt, modelMetaAll);
        }

        void DeleteAllChildren(GameObject parent)
        {
            int childCount = parent.transform.childCount;

            for (int i = childCount - 1; i >= 0; i--)
            {
                Transform child = parent.transform.GetChild(i);
                Destroy(child.gameObject);
            }
        }

    }
}
