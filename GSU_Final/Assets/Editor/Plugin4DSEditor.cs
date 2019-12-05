using UnityEngine;
using System.Collections;
using UnityEditor;

namespace unity4dv
{

    [CustomEditor(typeof(Plugin4DS))]
    public class Plugin4DSEditor : Editor
    {
        //bool showPath = false;

        public override void OnInspectorGUI()
        {
            Plugin4DS myTarget = (Plugin4DS)target;

            Undo.RecordObject(myTarget, "Inspector");

            BuildFilesInspector(myTarget);

            if (GUI.changed)
                EditorUtility.SetDirty(target);
        }



        private void BuildFilesInspector(Plugin4DS myTarget)
        {
            GUILayout.Space(10);
            myTarget.SourceType = (SOURCE_TYPE)EditorGUILayout.EnumPopup("Data Source", myTarget.SourceType);

            Rect rect = EditorGUILayout.BeginVertical(); ;
            if (myTarget.SourceType == SOURCE_TYPE.Local)
            {
                //rect to allow drag'n'drop
                myTarget.SequenceName = EditorGUILayout.TextField("Sequence Name", myTarget.SequenceName);
                EditorGUILayout.EndVertical();

                myTarget._dataInStreamingAssets = EditorGUILayout.Toggle("In Streaming Assets", myTarget._dataInStreamingAssets);
                if (!myTarget._dataInStreamingAssets)
                    myTarget.SequenceDataPath = EditorGUILayout.TextField("Data Path", myTarget.SequenceDataPath);
            }
            else //NETWORK
            {
                //myTarget.ConnexionHost = EditorGUILayout.TextField("Host", myTarget.ConnexionHost);
                //myTarget.ConnexionPort = EditorGUILayout.IntField("Port", myTarget.ConnexionPort);
                myTarget.SequenceName = EditorGUILayout.TextField("URL", myTarget.SequenceName);
                EditorGUILayout.EndVertical();
                myTarget.HTTPKeepInCache = EditorGUILayout.Toggle("Keep Data In Cache", myTarget.HTTPKeepInCache);
                if (Event.current.keyCode == KeyCode.Return)
                {
                    myTarget.Close();
                    myTarget.Preview();
                    myTarget.last_preview_time = System.DateTime.Now;
                }
            }

            GUILayout.Space(10);

            myTarget.AutoPlay = EditorGUILayout.Toggle("Auto Play", myTarget.AutoPlay);

            GUILayout.Space(10);

            GUIContent previewframe = new GUIContent("Preview Frame");
            Color color = GUI.color;
            if ((myTarget.LastActiveFrame != -1) && (myTarget.PreviewFrame < (int)myTarget.FirstActiveFrame || myTarget.PreviewFrame > (int)myTarget.LastActiveFrame))
                GUI.color = new Color(1, 0.6f, 0.6f);

            int frameVal = EditorGUILayout.IntSlider(previewframe, myTarget.PreviewFrame, 0, myTarget.SequenceNbOfFrames - 1);
            if (frameVal != myTarget.PreviewFrame)
            {
                myTarget.PreviewFrame = (int)frameVal;
                myTarget.Preview();
                myTarget.last_preview_time = System.DateTime.Now;
            }
            else
                myTarget.ConvertPreviewTexture();
            GUI.color = color;

            GUIContent activerange = new GUIContent("Active Range");
            float rangeMax = myTarget.LastActiveFrame == -1 ? myTarget.SequenceNbOfFrames - 1 : myTarget.LastActiveFrame;
            if (myTarget.LastActiveFrame == -1)
                GUI.color = new Color(0.5f, 0.7f, 2.0f);
            float firstActiveFrame = myTarget.FirstActiveFrame;
            EditorGUILayout.MinMaxSlider(activerange, ref firstActiveFrame, ref rangeMax, 0.0f, myTarget.SequenceNbOfFrames - 1);
            myTarget.FirstActiveFrame = (int)firstActiveFrame;
            if (rangeMax == myTarget.SequenceNbOfFrames - 1 && myTarget.FirstActiveFrame == 0)
                myTarget.LastActiveFrame = -1;
            else
                myTarget.LastActiveFrame = (int)rangeMax;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();

            if (myTarget.LastActiveFrame == -1)
            {
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Full Range", GUILayout.Width(80));
                GUI.color = color;
                EditorGUILayout.Space();
                myTarget.LastActiveFrame = -1;
            }
            else
            {
                myTarget.FirstActiveFrame = EditorGUILayout.IntField((int)myTarget.FirstActiveFrame, GUILayout.Width(50));
                EditorGUILayout.Space();
                myTarget.LastActiveFrame = EditorGUILayout.IntField((int)myTarget.LastActiveFrame, GUILayout.Width(50));
            }


            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            //myTarget.OutOfRangeMode = (OUT_RANGE_MODE)EditorGUILayout.EnumPopup("Out of Range Mode", myTarget.OutOfRangeMode);

            myTarget.SpeedRatio = EditorGUILayout.FloatField("Speed Ratio", myTarget.SpeedRatio);

            myTarget._debugInfo = EditorGUILayout.Toggle("Debug Info", myTarget._debugInfo);


            Event evt = Event.current;
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (rect.Contains(evt.mousePosition))
                        DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                    else
                    {
                        //EditorGUILayout.EndVertical();
                        return;
                    }

                    if (evt.type == EventType.DragPerform)
                    {
                        foreach (string path in DragAndDrop.paths)
                        {
                            string seqName = path.Substring(path.LastIndexOf("/") + 1);
                            string dataPath = path.Substring(0, path.LastIndexOf("/") + 1);

                            if (dataPath.Contains("StreamingAssets"))
                            {
                                myTarget._dataInStreamingAssets = true;
                                dataPath = dataPath.Substring(dataPath.LastIndexOf("StreamingAssets") + 16);
                                myTarget.SequenceDataPath = dataPath;
                            }
                            else
                            {
                                if (dataPath.Contains("Assets"))
                                {
                                    string message = "The sequence should be in \"Streaming Assets\" for a good application deployment";
                                    EditorUtility.DisplayDialog("Warning", message, "Close");
                                }
                                myTarget._dataInStreamingAssets = false;
                                myTarget.SequenceDataPath = dataPath;
                            }

                            myTarget.SequenceName = seqName;
                            myTarget.SourceType = SOURCE_TYPE.Local;

                            EditorUtility.SetDirty(target);

                            myTarget.Close();
                            myTarget.Preview();
                        }
                    }
                    break;
            }
        }

    }

}