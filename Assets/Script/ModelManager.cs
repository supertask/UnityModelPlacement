using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ModelPlacement
{
    public class ModelManager : MonoBehaviour
    {
        private const string BASE_URL = "http://127.0.0.1:8000/api";
        private const string DOWNLOADED_FILE_DIR = "AppData";

        ModelTransformGetter manager;

        void Start()
        {
            manager = new ModelTransformGetter();

            StartCoroutine(GetModelMetaForChatGPT());
        }

        IEnumerator GetModelMetaForChatGPT()
        {
            string url = $"{BASE_URL}/model/meta_all/for_chat_gpt";
            UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var text = request.downloadHandler.text;
                JArray meta = JArray.Parse(text);
                Debug.LogFormat("GetModelMetaForChatGPT: {0}, tex: {1}", meta.ToString(), text);
                //Debug.LogFormat("tex: {0}",text);

                //var modelTransform = manager.GetOrganizingModelJson(text); //heavy
                TextAsset textAsset = Resources.Load<TextAsset>("TestJson"); //for test
                var modelTransform = textAsset.text;
                Debug.Log(modelTransform);
                PlaceModel(modelTransform);
            }
            else
            {
                Debug.LogError("Error: " + request.error);
            }
        }

        public void PlaceModel(string jsonContent)
        {
            ModelDataList modelDataList = JsonUtility.FromJson<ModelDataList>(jsonContent);

            foreach (ModelData modelData in modelDataList.models)
            {
                //modelData.id
                GameObject model = GameObject.CreatePrimitive(PrimitiveType.Cube);

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

                model.transform.localScale = new Vector3(
                    modelData.transform.localScale[0],
                    modelData.transform.localScale[1],
                    modelData.transform.localScale[2]
                );
            }
        }

    }

}
