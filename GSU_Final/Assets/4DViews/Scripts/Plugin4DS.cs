//#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_WSA || UNITY_LUMIN
//#define USE_NATIVE_LIB
//#endif

using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace unity4dv
{

    public enum OUT_RANGE_MODE
    {
        Loop = 0,
        Reverse = 1,
        Stop = 2,
        Hide = 3
    }

    public enum SOURCE_TYPE
    {
        Local = 0,
        Network = 1
    }

    interface IPlugin4DSInterface
    {
        void Initialize();
        void Close();

        void Play(bool on);
        void GotoFrame(int frame);
    }


    public class Plugin4DS : MonoBehaviour, IPlugin4DSInterface
    {
#region Properties
        //-----------------------------//
        //-  PROPERTIES               -//
        //-----------------------------//
        public int CurrentFrame { get { return GetCurrentFrame(); } set { GotoFrame((int)value); } }
        public float Framerate { get { return GetFrameRate(); } }
        public int SequenceNbOfFrames { get { return GetSequenceNbFrames(); } }
        public int ActiveNbOfFrames { get { return GetActiveNbFrames(); } }
        public int FirstActiveFrame { get { return (int)_activeRangeMin; } set { _activeRangeMin = (float)value; } }
        public int LastActiveFrame { get { return (int)_activeRangeMax; } set { _activeRangeMax = (float)value; } }
        public TextureFormat TextureEncoding { get { return GetTextureFormat(); } }

        public bool AutoPlay { get { return _autoPlay; } set { _autoPlay = value; } }
        public bool IsPlaying { get { return _isPlaying; } set { _isPlaying = value; } }
        public bool IsInitialized { get { return _isInitialized; } }

        public string SequenceName { get { return _sequenceName; } set { _sequenceName = value; } }
        public string SequenceDataPath { get { return _mainDataPath; } set { _mainDataPath = value; } }

        public SOURCE_TYPE SourceType { get { return _sourceType; } set { _sourceType = value; } }

        //public string ConnexionHost { get { return _connexionHost; } set { _connexionHost = value; } }
        //public int ConnexionPort { get { return _connexionPort; } set { _connexionPort = value; } }

//        public OUT_RANGE_MODE OutOfRangeMode { get { return _outRangeMode; } set { SetOutRangeMode(value); } }

        public int PreviewFrame { get { return _previewFrame; } set { _previewFrame = value; } }

        public float SpeedRatio {  get { return _speedRatio; } set { _speedRatio = value; } }

        public int MeshBufferSize {   get { return Bridge4DS.GetMeshBufferSize(_dataSource.FDVUUID); } }
        public int ChunkBufferSize {  get { return Bridge4DS.GetChunkBufferSize(_dataSource.FDVUUID); } }
        public int MeshBufferMaxSize { get { return _meshBufferMaxSize; } set { _meshBufferMaxSize = value; } }
        public int ChunkBufferMaxSize { get { return _chunkBufferMaxSize; } set { _chunkBufferMaxSize = value; } }
        public int HTTPDownloadSize { get { return _HTTPDownloadSize; } set { _HTTPDownloadSize = value; } }
        public bool HTTPKeepInCache { get { return _HTTPKeepInCache; }  set { _HTTPKeepInCache = value; } }

        #endregion

        #region Events
        //-----------------------------//
        //-  EVENTS                   -//
        //-----------------------------//
        public delegate void EventFDV();
        public event EventFDV OnNewModel;
        public event EventFDV OnModelNotFound;
        //public event EventFDV onOutOfRange;
#endregion

#region classMembers
        //-----------------------------//
        //- Class members declaration -//
        //-----------------------------//

        //Path containing the 4DR data (edited in the unity editor panel)
        [SerializeField]
        private string _sequenceName;

        [SerializeField]
        private SOURCE_TYPE _sourceType = SOURCE_TYPE.Local;

        [SerializeField]
        private string _mainDataPath;
        public bool _dataInStreamingAssets = false;

        //[SerializeField]
        //private string _connexionHost;
        //[SerializeField]
        //private int _connexionPort = 80;

        [SerializeField]
        private int _meshBufferMaxSize = 10;
        [SerializeField]
        private int _chunkBufferMaxSize = 180;
        [SerializeField]
        private int _HTTPDownloadSize = 10000000;
        [SerializeField]
        private bool _HTTPKeepInCache = false;

        //Playback
        [SerializeField]
        private bool _autoPlay = true;
        [SerializeField]
        private OUT_RANGE_MODE _outRangeMode = OUT_RANGE_MODE.Loop;

        //Active Range
        [SerializeField]
        private float _activeRangeMin = 0;
        [SerializeField]
        private float _activeRangeMax = -1;

        //Infos
        public bool _debugInfo = false;
        private float _decodingFPS = 0f;
        private int _lastDecodingId = 0;
        private System.DateTime _lastDecodingTime;
        private float _updatingFPS = 0f;
        private int _lastUpdatingId = 0;
        private System.DateTime _lastUpdatingTime;
        private int _totalFramesPlayed = 0;
        private System.DateTime _playDate;

        //4D source
        private DataSource4DS _dataSource = null;
        [SerializeField]
        private int _lastModelId = -1;

        //Mesh and texture objects
        private Mesh[] _meshes = null;
        private Texture2D[] _textures = null;
        private MeshFilter _meshComponent;
        private Renderer _rendererComponent;

        //Receiving geometry and texture buffers
        private Vector3[] _newVertices;
        private Vector2[] _newUVs;
        private int[] _newTriangles;
        private byte[] _newTextureData;
        private Vector3[] _newNormals = null;
        private GCHandle _newVerticesHandle;
        private GCHandle _newUVsHandle;
        private GCHandle _newTrianglesHandle;
        private GCHandle _newTextureDataHandle;
        private GCHandle _newNormalsHandle;

        //Mesh and texture multi-buffering (optimization)
        private int _nbGeometryBuffers = 2;
        private int _currentGeometryBuffer;
        private int _nbTextureBuffers = 2;
        private int _currentTextureBuffer;

        //time a latest update
        //private float           _prevUpdateTime=0.0f;
        private bool _newMeshAvailable = false;
        private bool _isSequenceTriggerON = false;
        private float _triggerRate = 0.3f;

        //pointer to the mesh Collider, if present (=> will update it at each frames for collisions)
        private MeshCollider _meshCollider;

        //Has the plugin been initialized
        [SerializeField]
        private bool _isInitialized = false;
        [SerializeField]
        private bool _isPlaying = false;

        [SerializeField]
        private int _previewFrame = 0;
        public System.DateTime last_preview_time = System.DateTime.Now;

        [SerializeField]
        private int _nbFrames = 0;
        [SerializeField]
        private float _speedRatio = 1.0f;

        private int _nbVertices;
        private int _nbTriangles;

        private const int MAX_SHORT = 65535;

        #endregion

#region methods
        //-----------------------------//
        //- Class methods implement.  -//
        //-----------------------------//



        public void Initialize()
        {
            //Initialize already called successfully
            if (_isInitialized == true)
                return;

            if (_dataSource == null)
            {
                int key = 0;
                //if (_sourceType == SOURCE_TYPE.Network)
                //{
                //    key = Bridge4DS.CreateConnection(_connexionHost, _connexionPort);
                //}

                //Creates data source from the given path 
                _dataSource = DataSource4DS.CreateDataSource(key, _sequenceName, _dataInStreamingAssets, _mainDataPath, (int)_activeRangeMin, (int)_activeRangeMax, _outRangeMode);
                if (_dataSource == null)
                {
                    OnModelNotFound?.Invoke();
                    return;
                }
            }

            _lastModelId = -1;

            _meshComponent = GetComponent<MeshFilter>();
            _rendererComponent = GetComponent<Renderer>();
            _meshCollider = GetComponent<MeshCollider>();

            _nbFrames = Bridge4DS.GetSequenceNbFrames(_dataSource.FDVUUID);

            Bridge4DS.SetSpeed(_dataSource.FDVUUID, _speedRatio);

            if (_sourceType == SOURCE_TYPE.Network)
            {
                Bridge4DS.SetHTTPDownloadSize(_dataSource.FDVUUID, _HTTPDownloadSize);
                Bridge4DS.SetHTTPKeepInCache(_dataSource.FDVUUID, _HTTPKeepInCache);
            }

            Bridge4DS.SetChunkBufferMaxSize(_dataSource.FDVUUID, _chunkBufferMaxSize);
            Bridge4DS.SetMeshBufferMaxSize(_dataSource.FDVUUID, _meshBufferMaxSize);


            //Allocates geometry buffers
            AllocateGeometryBuffers(ref _newVertices, ref _newUVs, ref _newNormals, ref _newTriangles, _dataSource.MaxVertices, _dataSource.MaxTriangles);

            //Allocates texture pixel buffer
            int pixelBufferSize = _dataSource.TextureSize * _dataSource.TextureSize / 2;    //default is 4 bpp
            if (_dataSource.TextureFormat == TextureFormat.PVRTC_RGB2 )  //pvrtc2 is 2bpp
                pixelBufferSize /= 2;
            if (_dataSource.TextureFormat == TextureFormat.ASTC_RGBA_8x8)
            {
                int blockSize = 8;
                int xblocks = (_dataSource.TextureSize + blockSize - 1) / blockSize;
                pixelBufferSize = xblocks * xblocks * 16;
            }
            _newTextureData = new byte[pixelBufferSize];

            //Gets pinned memory handle
            _newVerticesHandle = GCHandle.Alloc(_newVertices, GCHandleType.Pinned);
            _newUVsHandle = GCHandle.Alloc(_newUVs, GCHandleType.Pinned);
            _newTrianglesHandle = GCHandle.Alloc(_newTriangles, GCHandleType.Pinned);
            _newTextureDataHandle = GCHandle.Alloc(_newTextureData, GCHandleType.Pinned);
            _newNormalsHandle = GCHandle.Alloc(_newNormals, GCHandleType.Pinned);

            //Allocates objects buffers for double buffering
            _meshes = new Mesh[_nbGeometryBuffers];
            _textures = new Texture2D[_nbTextureBuffers];

            for (int i = 0; i < _nbGeometryBuffers; i++)
            {
                //Mesh
                Mesh mesh = new Mesh();
                if (_dataSource.MaxVertices > MAX_SHORT)
                    mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                mesh.MarkDynamic(); //Optimize mesh for frequent updates. Call this before assigning vertices. 
                mesh.vertices = _newVertices;
                mesh.uv = _newUVs;
                mesh.triangles = _newTriangles;
                mesh.normals = _newNormals;

                Bounds newBounds = mesh.bounds;
                newBounds.extents = new Vector3(4, 4, 4);
                mesh.bounds = newBounds;
                _meshes[i] = mesh;
            }
            
            for (int i = 0; i < _nbTextureBuffers; i++)
            {
                //Texture
#if UNITY_2019_1_OR_NEWER
                if (_dataSource.TextureFormat == TextureFormat.ASTC_RGBA_8x8)   //since unity 2019 ASTC RGBA is no more supported
                    _dataSource.TextureFormat = TextureFormat.ASTC_8x8;
#endif
                Texture2D texture = new Texture2D(_dataSource.TextureSize, _dataSource.TextureSize, _dataSource.TextureFormat, false)
                {
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear
                };
                texture.Apply(); //upload to GPU
                _textures[i] = texture;
            }

            _currentGeometryBuffer = _currentTextureBuffer = 0;

            _nbFrames = Bridge4DS.GetSequenceNbFrames(_dataSource.FDVUUID);
            _isInitialized = true;
        }


        void Uninitialize()
        {
            if (!_isInitialized)
                return;

            //Releases sequence
            if (_dataSource != null) Bridge4DS.DestroySequence(_dataSource.FDVUUID);

            //Releases memory
            if (_newVerticesHandle.IsAllocated) _newVerticesHandle.Free();
            if (_newUVsHandle.IsAllocated) _newUVsHandle.Free();
            if (_newTrianglesHandle.IsAllocated) _newTrianglesHandle.Free();
            if (_newTextureDataHandle.IsAllocated) _newTextureDataHandle.Free();
            if (_newNormalsHandle.IsAllocated) _newNormalsHandle.Free();

            if (_meshes != null) {
                for (int i = 0; i < _meshes.Length; i++)
                    DestroyImmediate(_meshes[i]);
                _meshes = null;
            }
            if (_textures != null) {
                for (int i = 0; i < _textures.Length; i++)
                    DestroyImmediate(_textures[i]);
                _textures = null;
            }

            _dataSource = null;
            _newVertices = null;
            _newUVs = null;
            _newTriangles = null;
            _newNormals = null;
            _newTextureData = null;

            _isSequenceTriggerON = false;
            _isInitialized = false;

#if UNITY_EDITOR
            EditorApplication.pauseStateChanged -= HandlePauseState;
#endif
        }


        void OnDestroy()
        {
            Close();
        }


        void Awake()
        {
            if (_isInitialized)
                Uninitialize();

            if (_sequenceName != "")
                Initialize();

            //Hide preview mesh
            if (_meshComponent != null)
                _meshComponent.mesh = null;

#if UNITY_EDITOR
            EditorApplication.pauseStateChanged +=HandlePauseState;
#endif
        }


        void Start()
        {
            if (!_isInitialized &&   _sequenceName != "") 
                Initialize();

            if (_dataSource == null)
                return;

            //launch sequence play
            if (_autoPlay){
                Play(true);
            }
        }



        //Called every frame
        //Get the geometry from the plugin and update the unity gameobject mesh and texture
        void Update()
        {
            if (!_isInitialized && _sequenceName != "")
                Initialize();

            if (_dataSource == null)
            {
                Debug.LogError("No data source");
                return;
            }
            //everything is in UpdateMesh(), which called by the SequenceTrigger coroutine

#if UNITY_EDITOR
            //called when the step button in editor is clicked
            if (EditorApplication.isPaused)
            {
                GotoFrame((GetCurrentFrame() + 1) % GetSequenceNbFrames());
            }
#endif

            if (_newMeshAvailable)
            {
                //Get current object buffers (double buffering)
                Mesh mesh = _meshes[_currentGeometryBuffer];
                Texture2D texture = _textures[_currentTextureBuffer];

                //Optimize mesh for frequent updates. Call this before assigning vertices.
                //Seems to be useless :(
                mesh.MarkDynamic();

                //Update geometry
                mesh.vertices = _newVertices;
                mesh.uv = _newUVs;
                if (_nbTriangles == 0)  //case empty mesh
                    mesh.triangles = null;
                else
                    mesh.triangles = _newTriangles;
                
                mesh.normals = _newNormals;
                
                mesh.UploadMeshData(false); //Good optimization ! nbGeometryBuffers must be = 1

                //Update texture
                texture.LoadRawTextureData(_newTextureData);
                texture.Apply();

                //Assign current mesh buffers and texture
                _meshComponent.sharedMesh = mesh;

                if (_rendererComponent.sharedMaterial.HasProperty("_BaseMap"))
                    _rendererComponent.sharedMaterial.SetTexture("_BaseMap", texture);
                else if (_rendererComponent.sharedMaterial.HasProperty("_BaseColorMap"))
                    _rendererComponent.sharedMaterial.SetTexture("_BaseColorMap", texture);
                else if (_rendererComponent.sharedMaterial.HasProperty("_UnlitColorMap"))
                    _rendererComponent.sharedMaterial.SetTexture("_UnlitColorMap", texture);
                else
                {
#if UNITY_EDITOR
                    var tempMaterial = new Material(_rendererComponent.sharedMaterial);
                    tempMaterial.mainTexture = texture;
                    _rendererComponent.sharedMaterial = tempMaterial;
#else
                    _rendererComponent.material.mainTexture = texture;
#endif
                }

                //Switch buffers
                _currentGeometryBuffer = (_currentGeometryBuffer + 1) % _nbGeometryBuffers;
                _currentTextureBuffer = (_currentTextureBuffer + 1) % _nbTextureBuffers;

                //Send event
                OnNewModel?.Invoke();

                _newMeshAvailable = false;

                if (_meshCollider && _meshCollider.enabled)
                    _meshCollider.sharedMesh = mesh;
                //_updateCollider = !_updateCollider;

                _totalFramesPlayed++;
                if (_debugInfo)
                {
                    double timeInMSeconds = System.DateTime.Now.Subtract(_lastUpdatingTime).TotalMilliseconds;
                    _lastUpdatingId++;
                    if (timeInMSeconds > 500f)
                    {
                        _updatingFPS = (float)((float)(_lastUpdatingId) / timeInMSeconds * 1000f);
                        _lastUpdatingTime = System.DateTime.Now;
                        _lastUpdatingId = 0;
                    }
                }
            }
        }


        private void UpdateMesh()
        {
            if (_dataSource == null)
                return;

            //Get the new model
            int modelId = Bridge4DS.UpdateModel(_dataSource.FDVUUID,
                                                      _newVerticesHandle.AddrOfPinnedObject(),
                                                      _newUVsHandle.AddrOfPinnedObject(),
                                                      _newTrianglesHandle.AddrOfPinnedObject(),
                                                      _newTextureDataHandle.AddrOfPinnedObject(),
                                                      _newNormalsHandle.AddrOfPinnedObject(),
                                                      _lastModelId,
                                                      ref _nbVertices,
                                                      ref _nbTriangles);

            //Check if there is model
            if (!_newMeshAvailable)
                _newMeshAvailable = (modelId != -1 && modelId != _lastModelId);

            if (modelId == -1) modelId = _lastModelId;
            else _lastModelId = modelId;

            if (_debugInfo)
            {
                double timeInMSeconds = System.DateTime.Now.Subtract(_lastDecodingTime).TotalMilliseconds;
                if (_lastDecodingId == 0 || timeInMSeconds > 500f)
                {
                    _decodingFPS = (float)((double)(Mathf.Abs((float)(modelId - _lastDecodingId))) / timeInMSeconds) * 1000f;
                    _lastDecodingTime = System.DateTime.Now;
                    _lastDecodingId = modelId;
                }
            }
        }


        //manage the UpdateMesh() call to have it triggered by the sequence framerate
        private IEnumerator SequenceTrigger()
        {
            float duration = (_triggerRate / _dataSource.FrameRate);

            //infinite loop to keep executing this coroutine
            while (true)
            {
                UpdateMesh();
                yield return new WaitForSeconds(duration);
            }
        }


#if UNITY_EDITOR
        private void HandlePauseState(PauseState state)
        {
            Play(state>0);
        }
#endif

        //Public functions
        public void Play(bool on)
        {
            if (on)
            {
                if (_isSequenceTriggerON == false)
                {
                    Bridge4DS.Play(_dataSource.FDVUUID, on);
                    StartCoroutine("SequenceTrigger");
                    _isSequenceTriggerON = true;
                    _totalFramesPlayed = 0;
                    _playDate = System.DateTime.Now;
                }
            }
            else
            {
                if (_isSequenceTriggerON == true)
                {
                    Bridge4DS.Play(_dataSource.FDVUUID, on);
                    StopCoroutine("SequenceTrigger");
                    _isSequenceTriggerON = false;
                }
            }
            _isPlaying = on;
        }
		

		public void Stop()
        {
            if (_isSequenceTriggerON == true)
            {
                Bridge4DS.Stop(_dataSource.FDVUUID);
                StopCoroutine("SequenceTrigger");
                _isSequenceTriggerON = false;
            }
            _isPlaying = false;
        }


        public void Close()
        {
            Stop();
            Uninitialize();
        }


        public void GotoFrame(int frame)
        {
            bool wasPlaying = _isPlaying;
            Play(false);
            Bridge4DS.GotoFrame(_dataSource.FDVUUID, frame);
            Play(wasPlaying);
            UpdateMesh();
        }


        private int GetSequenceNbFrames()
        {
            if (_dataSource != null)
                return Bridge4DS.GetSequenceNbFrames(_dataSource.FDVUUID);
            else
                return _nbFrames;
        }

        private int GetActiveNbFrames()
        {
            return (int)_activeRangeMax - (int)_activeRangeMin + 1;
        }

        private int GetCurrentFrame()
        {
            if (_lastModelId < 0)
                return 0;
            else
                return _lastModelId;
        }

        private float GetFrameRate()
        {
            return (_dataSource == null) ? 0.0f : _dataSource.FrameRate;
        }

//        private void SetOutRangeMode(OUT_RANGE_MODE mode)
//        {
//            if (_dataSource != null)
//                Bridge4DS.ChangeOutRangeMode(_dataSource.FDVUUID, mode);
//            _outRangeMode = mode;
//        }


        private TextureFormat GetTextureFormat()
        {
            return _dataSource.TextureFormat;
        }


        void OnGUI()
        {
            if (_debugInfo)
            {
                double delay = System.DateTime.Now.Subtract(_playDate).TotalMilliseconds - ((float)(_totalFramesPlayed) * 1000 / GetFrameRate());
                string decoding = _decodingFPS.ToString("00.00") + " fps";
                string updating = _updatingFPS.ToString("00.00") + " fps";
                delay /= 1000;
                if (!_isPlaying)
                {
                    delay = 0f;
                    decoding = "paused";
                    updating = "paused";
                }
                int top = 20;
                GUIStyle title = new GUIStyle();
                title.normal.textColor = Color.white;
                title.fontStyle = FontStyle.Bold;
                GUI.Button(new Rect(Screen.width - 210, top - 10, 200, 330), "");
                GUI.Label(new Rect(Screen.width - 200, top, 190, 20), "Sequence ", title);
                GUI.Label(new Rect(Screen.width - 200, top += 15, 190, 20), "Length: " + ((float)GetSequenceNbFrames() / GetFrameRate()).ToString("00.00") + " sec");
                GUI.Label(new Rect(Screen.width - 200, top += 15, 190, 20), "Nb Frames: " + GetSequenceNbFrames() + " frames");
                GUI.Label(new Rect(Screen.width - 200, top += 15, 190, 20), "Frame rate: " + GetFrameRate().ToString("00.00") + " fps");
                GUI.Label(new Rect(Screen.width - 200, top += 15, 190, 20), "Max vertices: " + _dataSource.MaxVertices);
                GUI.Label(new Rect(Screen.width - 200, top += 15, 190, 20), "Max triangles: " + _dataSource.MaxTriangles);
                GUI.Label(new Rect(Screen.width - 200, top += 15, 190, 20), "Texture format: " + _dataSource.TextureFormat);
                GUI.Label(new Rect(Screen.width - 200, top += 15, 190, 20), "Texture size: " + _dataSource.TextureSize + "x" + _dataSource.TextureSize + "px");
                GUI.Label(new Rect(Screen.width - 200, top += 25, 190, 20), "Current Mesh", title);
                GUI.Label(new Rect(Screen.width - 200, top += 15, 190, 20), "Nb vertices: " + _nbVertices);
                GUI.Label(new Rect(Screen.width - 200, top += 15, 190, 20), "Nb triangles: " + _nbTriangles);
                GUI.Label(new Rect(Screen.width - 200, top += 25, 190, 20), "Playback", title);
                GUI.Label(new Rect(Screen.width - 200, top += 15, 190, 20), "Time: " + ((float)(CurrentFrame) / GetFrameRate()).ToString("00.00") + " sec");
                GUI.Label(new Rect(Screen.width - 200, top += 15, 190, 20), "Decoding rate: " + decoding);
                GUI.Label(new Rect(Screen.width - 200, top += 15, 190, 20), "Decoding delay: " + delay.ToString("00.00") + " sec");
                GUI.Label(new Rect(Screen.width - 200, top += 15, 190, 20), "Updating rate: " + updating);
            }
        }


        public void Preview()
        {
            //save params values
            int nbGeometryTMP = _nbGeometryBuffers;
            int nbTextureTMP = _nbTextureBuffers;
            bool debugInfoTMP = _debugInfo;

            //set params values for preview
            _nbGeometryBuffers = 1;
            _nbTextureBuffers = 1;
            _debugInfo = false;

            if (_isInitialized && _dataSource == null)
                _isInitialized = false;

            //get the sequence
            Initialize();

            if (_isInitialized)
            {
                //set mesh to the preview frame
                GotoFrame(_previewFrame);
                Update();
                //Assign current texture to new material to have it saved
                var tempMaterial = new Material(_rendererComponent.sharedMaterial)
                {
                    mainTexture = _rendererComponent.sharedMaterial.mainTexture
                };
                _rendererComponent.sharedMaterial = tempMaterial;
            }

            //restore params values
            _nbGeometryBuffers = nbGeometryTMP;
            _nbTextureBuffers = nbTextureTMP;
            _debugInfo = debugInfoTMP;
        }



        public void ConvertPreviewTexture()
        {
            System.DateTime current_time = System.DateTime.Now;
            if (_rendererComponent != null && _rendererComponent.sharedMaterial.mainTexture != null)
            {
                if (((System.TimeSpan)(current_time - last_preview_time)).TotalMilliseconds < 1000
                    || ((Texture2D)_rendererComponent.sharedMaterial.mainTexture).format == TextureFormat.DXT1)
                    return;

                last_preview_time = current_time;

                if (_rendererComponent != null)
                {
                    Texture2D tex = (Texture2D)_rendererComponent.sharedMaterial.mainTexture;
                    if (tex && tex.format != TextureFormat.RGBA32)
                    {
                        Color32[] pix = tex.GetPixels32();
                        Texture2D textureRGBA = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false)
                        {
                            wrapMode = TextureWrapMode.Clamp
                        };
                        textureRGBA.SetPixels32(pix);
                        textureRGBA.Apply();

                        _rendererComponent.sharedMaterial.mainTexture = textureRGBA;
                    }
                }
            }
        }


        private void AllocateGeometryBuffers(ref Vector3[] verts, ref Vector2[] uvs, ref Vector3[] norms, ref int[] tris, int nbMaxVerts, int nbMaxTris)
        {
            verts = new Vector3[nbMaxVerts];
            uvs = new Vector2[nbMaxVerts];
            tris = new int[nbMaxTris * 3];
            norms = new Vector3[nbMaxVerts];
        }
    }

    #endregion

}

