using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AICommand;


namespace ModelPlacement
{

    public struct Model
    {
        public string name;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 localScale;
    }

    public class ModelTransformGetter
    {
        private string promptPath = "furniturePrompt";

        public ModelTransformGetter()
        {

        }

        public string GetOrganizingModelJson(string modelMeta)
        {
            //var prompt = "[{'model_name': 'tub', 'meta': [{'bounding_box': {'center': [3.6707544, 1.9304, -2.7820005], 'size': [7.3327311, 3.8608, 5.307619]}}, {'bounding_box': {'center': [-2.844715, 0.1798225, -0.206161], 'size': [1.70001, 0.440445, 0.7]}}, `";
            var prompt = this.WrapPrompt(modelMeta, "Please try to make the room as girl-friendly as possible.");
            Debug.Log("prompt: " + prompt);
            var organizingModelJson = OpenAIUtil.InvokeChat(prompt);
            //var organizingModelJson = "";
            return organizingModelJson;
        }

        public string WrapPrompt(string modelMeta, string rules)
        {
            return this.LoadPrompt(promptPath).Replace("{roomVibe}", rules).Replace("{modelMetaJson}", modelMeta);
        }


        private string LoadPrompt(string fileName)
        {
            TextAsset textAsset = Resources.Load<TextAsset>(fileName);

            if (textAsset == null)
            {
                Debug.LogError($"Text file is not found: {fileName}");
                return "";
            }

            return textAsset.text;
        }
    }
}
