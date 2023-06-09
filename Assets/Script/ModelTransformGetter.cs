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

        public string GetTransformJson(string modelMeta)
        {
            var prompt = this.WrapPrompt(modelMeta, 
@"");
            var organizingModelJson = OpenAIUtil.InvokeChat(prompt);
            return organizingModelJson;
        }

        public string WrapPrompt(string modelMeta, string rules)
        {
            return this.LoadPrompt(promptPath).Replace("{rules}", rules).Replace("{modelMetaJson}", modelMeta);
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
