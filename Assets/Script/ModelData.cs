namespace ModelPlacement
{
    [System.Serializable]
    public class TransformData
    {
        public float[] position;
        public float[] rotation;
        public float[] localScale;
    }

    [System.Serializable]
    public class ModelData
    {
        public string id;
        public string model_name;
        public TransformData transform;
    }

    [System.Serializable]
    public class ModelDataList
    {
        public ModelData[] models;
    }
}
