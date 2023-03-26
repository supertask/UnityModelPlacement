namespace ModelPlacement
{
    [System.Serializable]
    public class TransformData
    {
        public float[] position;
        public float[] rotation;
    }

    [System.Serializable]
    public class ModelData
    {
        public string model_id;
        public string model_child_id;
        //public string model_name;
        public TransformData transform;
    }

    [System.Serializable]
    public class ModelDataList
    {
        public ModelData[] models;
    }
}
