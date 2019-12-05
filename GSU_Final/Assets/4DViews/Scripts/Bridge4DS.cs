
using UnityEngine;
using System.Runtime.InteropServices;

//-----------------Bridge4DS-----------------//

namespace unity4dv
{

    //Imports the native plugin functions.

    public class Bridge4DS
    {
        #if UNITY_IPHONE && !UNITY_EDITOR
            private const string IMPORT_NAME = "__Internal";  
        #else //Android & Desktop
            private const string IMPORT_NAME = "BridgeCodec4DS";
        #endif


        //Inits the plugin (sequencemanager, etc.)
        [DllImport(IMPORT_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int CreateSequence(int key, [MarshalAs(UnmanagedType.LPStr)] string dataPath, int rangeBegin, int rangeEnd, OUT_RANGE_MODE outRangeMode);

        [DllImport(IMPORT_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        //Stops the plugin and releases memory (sequencemanager, etc.)
        public static extern void DestroySequence(int key);

        [DllImport(IMPORT_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        //Starts or stops the playback
        public static extern void Play(int key, bool on);
		
		[DllImport(IMPORT_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        //Stops the playback
        public static extern void Stop(int key);

        [DllImport(IMPORT_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        //Gets the new model from plugin
        public static extern int UpdateModel(int key,
                                                System.IntPtr ptrVertices,
                                                System.IntPtr ptrUVs,
                                                System.IntPtr ptrTriangles,
                                                System.IntPtr texture,
                                                System.IntPtr normals,
                                                int lastModelId,
                                                ref int nbVertices,
                                                ref int nbTriangles);

//        [DllImport(IMPORT_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
//        public static extern bool OutOfRangeEvent(int key);

        [DllImport(IMPORT_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        //Gets the 4DR texture image size
        public static extern int GetTextureSize(int key);

        [DllImport(IMPORT_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        //Gets the 4DR texture encoding
        public static extern int GetTextureEncoding(int key);

        [DllImport(IMPORT_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetSequenceMaxVertices(int key);

        [DllImport(IMPORT_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetSequenceMaxTriangles(int key);

        [DllImport(IMPORT_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern float GetSequenceFramerate(int key);

        [DllImport(IMPORT_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetSequenceNbFrames(int key);

        [DllImport(IMPORT_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetSequenceCurrentFrame(int key);

        [DllImport(IMPORT_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void GotoFrame(int key, int frame);

        [DllImport(IMPORT_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void SetSpeed(int key, float speedRatio);

        [DllImport(IMPORT_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetChunkBufferSize(int key);

        [DllImport(IMPORT_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetMeshBufferSize(int key);

        [DllImport(IMPORT_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void SetChunkBufferMaxSize(int key, int size);

        [DllImport(IMPORT_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void SetMeshBufferMaxSize(int key, int size);

        [DllImport(IMPORT_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetHTTPDownloadSize(int key);

        [DllImport(IMPORT_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void SetHTTPDownloadSize(int key, int size);

        [DllImport(IMPORT_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern bool GetHTTPKeepInCache(int key);

        [DllImport(IMPORT_NAME, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void SetHTTPKeepInCache(int key, bool val);


    } //class Bridge4DS
} //namespace unity4DV