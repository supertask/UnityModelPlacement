using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Dummiesman;
using Cysharp.Threading.Tasks;  

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
        /*
        public void PlaceModel(string jsonContent)
        {
            ModelDataList modelDataList = JsonUtility.FromJson<ModelDataList>(jsonContent);

            _numberOfDownloadingTasks = modelDataList.models.Length;
            foreach (ModelData modelData in modelDataList.models)
            {
                StartCoroutine(shapeNetCaller.DownloadModel(modelData, OnCompleteDownloading));
            }
        }
        */

        public async void PlaceModel(string jsonContent)
        {
            ModelDataList modelDataList = JsonUtility.FromJson<ModelDataList>(jsonContent);
            List<UniTask> downloadTasks = new List<UniTask>();

            foreach (ModelData modelData in modelDataList.models)
            {
                downloadTasks.Add(shapeNetCaller.DownloadModel(modelData, OnCompleteDownloading));
            }

            await UniTask.WhenAll(downloadTasks);
            Debug.Log("Finish!");
        }

        public void AskAndPlaceModel(string prompt)
        {
            string modelTransform;
            if (isDebug) {
                TextAsset textAsset = Resources.Load<TextAsset>("TestJson"); //for test
                modelTransform = textAsset.text;
            }
            else
            {
                modelTransform = manager.GetOrganizingModelJson(prompt); //heavy
            }
            if (modelTransform == "") { return; }
            PlaceModel(modelTransform);
        }

        private void OnCompleteDownloading(ModelData modelData, ModelResourcePath modelResourcePath)
        {
            // 3Dƒ‚ƒfƒ‹‚Ì¶¬
            GameObject model = new OBJLoader().Load(modelResourcePath.objPath, modelResourcePath.mtlPath);
            model.name = modelData.model_name;

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
            model.transform.localScale = Vector3.one;
            
            model.transform.parent = this.transform;
        }

        public async void UpdateModelAndPosition()
        {
            DeleteAllChildren(this.gameObject);
            int seed = Random.Range(0, 10000);

            bool success = await shapeNetCaller.UpdateSeedApiRequest(seed);
            if (!success) { return; }

            var prompt = await shapeNetCaller.GetModelMetaForChatGPT();
            if (prompt == null) { return; }

            AskAndPlaceModel(prompt);
        }

        public async void UpdatePosition()
        {
            DeleteAllChildren(this.gameObject);

            var prompt = await shapeNetCaller.GetModelMetaForChatGPT();
            AskAndPlaceModel(prompt);
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
