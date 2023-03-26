using UnityEditor;
using UnityEngine;

namespace ModelPlacement
{
    [CustomEditor(typeof(ModelManager))]
    public class ModelManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            ModelManager manager = (ModelManager)target;
            if (GUILayout.Button("Update model type & position"))
            {
                manager.UpdateModelAndPosition();
            }
            if (GUILayout.Button("Update model position"))
            {
                manager.UpdatePosition();
            }
            if (GUILayout.Button("Delete model cache"))
            {
                manager.ShapeNetCaller.DeleteCache();
            }
        }
    }
}