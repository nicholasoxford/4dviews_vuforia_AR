
using UnityEngine;

//-----------------DataSource4DS-----------------//
//Creates a 4D sequence from a path.
//If the path is a directory, DataSource4DS looks for the best supported format.
//If path is a sequence.xml file, DataSource4DS creates directly a sequence
//from this file without checking format compatibility.

namespace unity4dv
{


    public class DataSource4DS
    {
        public int FDVUUID;
        public TextureFormat TextureFormat;
        public int TextureSize;
        public int MaxVertices;
        public int MaxTriangles;
        public float FrameRate;

        //Static constructor: creates a data source or returns null when no source can be created
        static public DataSource4DS CreateDataSource(int key, string sequenceName, bool dataInStreamingAssets, string mainPath, int activeRangeBegin, int activeRangeLastFrame, OUT_RANGE_MODE outRangeMode)
        {
            bool success = false;
            string rootpath;
            
            if (!sequenceName.StartsWith("http") && key == 0 && dataInStreamingAssets)
            {
                rootpath = Application.streamingAssetsPath + "/" + mainPath + sequenceName;

                //ANDROID STREAMING ASSETS => need to copy the data somewhere else on device to acces it, beacause it is currently in jar file
                if (rootpath.StartsWith("jar"))
                {
                    WWW www = new WWW(rootpath);
                    //yield return www; //can't do yield here, not really blocking beacause the data is local
                    while (!www.isDone) ;

                    if (!string.IsNullOrEmpty(www.error))
                    {
                        Debug.LogError("PATH : " + rootpath);
                        Debug.LogError("Can't read data in streaming assets: "+www.error);
                    }
                    else
                    {
                        //copy data on device
                        rootpath = Application.persistentDataPath + "/" + sequenceName;
                        if (!System.IO.File.Exists(rootpath))
                        {
                            Debug.Log("4DVIEWS: NEW Roopath: " + rootpath);
                            System.IO.FileStream fs = System.IO.File.Create(rootpath);
                            fs.Write(www.bytes, 0, www.bytesDownloaded);
                            Debug.Log("4DVIEWS: data copied");
                            fs.Dispose();
                        }
                    }
                }

                Debug.Log("Create Instance");
                DataSource4DS instance = new DataSource4DS(key, rootpath, activeRangeBegin, activeRangeLastFrame, outRangeMode, ref success);
                if (success)
                {
                    Debug.Log("Instance Created ");
                    return instance;
                }
                else
                {
                    Debug.LogError("FDV Error: cannot find data source at location " + rootpath);
                    return null;
                }

            }
            else
            {
                if (sequenceName.StartsWith("http"))
                    rootpath = sequenceName;
                else
                    rootpath = mainPath + sequenceName;
                DataSource4DS instance = new DataSource4DS(key, rootpath, activeRangeBegin, activeRangeLastFrame, outRangeMode, ref success);
                if (success)
                    return instance;
                else
                {
                    Debug.LogError("FDV Error: cannot find data source at " + rootpath);
                    return null;
                }
            }
        }

        //private constructor
        private DataSource4DS(int key, string rootpath, int activeRangeBegin, int activeRangeEnd, OUT_RANGE_MODE outRangeMode, ref bool success)
        {
            this.FDVUUID = 0;
            success = true;

            //Create sequence with native plugin
            this.FDVUUID = Bridge4DS.CreateSequence(key, rootpath, activeRangeBegin, activeRangeEnd, outRangeMode);

            if (this.FDVUUID == 0)
                success = false;

            //Get sequence info
            if (success)
            {
                this.TextureSize = Bridge4DS.GetTextureSize(this.FDVUUID);
                if (this.TextureSize == 0)
                    this.TextureSize = 1024;    //put 1024 by default => will crash if we have 2048 texture and it's not written in xml fi

                int textureEncoding = Bridge4DS.GetTextureEncoding(this.FDVUUID);

                switch (textureEncoding)
                {
                    case 5:
					case 120:
                        this.TextureFormat = TextureFormat.ETC_RGB4;
                        break;
                    case 6:
					case 130:
                        this.TextureFormat = TextureFormat.PVRTC_RGB4;
                        break;
                    case 4:
					case 131:
                        this.TextureFormat = TextureFormat.PVRTC_RGB2;
                        break;
                    case 1:
					case 100:
                        this.TextureFormat = TextureFormat.DXT1;
                        break;
                    case 8:
					case 164:
                        this.TextureFormat = TextureFormat.ASTC_RGBA_8x8; 
                        break;
                    default:
#if UNITY_IPHONE
				this.TextureFormat = TextureFormat.PVRTC_RGB4;
#elif UNITY_ANDROID
                    this.TextureFormat = TextureFormat.ETC_RGB4;
#else
                        this.TextureFormat = TextureFormat.DXT1;
#endif
                        break;
                }

                this.MaxVertices = Bridge4DS.GetSequenceMaxVertices(this.FDVUUID);
                if (this.MaxVertices == 0)
                    this.MaxVertices = 65535;
                this.MaxTriangles = Bridge4DS.GetSequenceMaxTriangles(this.FDVUUID);
                if (this.MaxTriangles == 0)
                    this.MaxTriangles = 65535;
                this.FrameRate = (float)Bridge4DS.GetSequenceFramerate(this.FDVUUID);
            }
        }
 
    }

}