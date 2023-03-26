using System.Collections.Generic;

namespace ModelPlacement
{

    [System.Serializable]
    public class ModelSize
    {
        public List<float> center;
        public List<float> size;
    }

    [System.Serializable]
    public class ModelPath
    {
        public string meta;
        public string obj;
        public string mtl;
    }

    [System.Serializable]
    public class ModelApiResponse
    {
        public string model_child_id;
        public ModelSize real_world_size;
        public float model_scale;
        public ModelPath model_path;
        public List<string> texture_paths;
    }

}