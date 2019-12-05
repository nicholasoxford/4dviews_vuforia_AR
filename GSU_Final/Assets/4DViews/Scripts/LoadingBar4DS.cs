using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using unity4dv;

public class LoadingBar4DS : MonoBehaviour
{
    public Plugin4DS _plugin4DS;

    public RectTransform _currentFrame;
    public RectTransform _meshBuffer;
    public RectTransform _chunkBuffer;

    private float _unit = 1;

    private RectTransform mbDuplicate = null;
    private RectTransform cbDuplicate = null;

    private RectTransform  background;

    void Start()
    {
        background = GetComponent<RectTransform>();
        if (_plugin4DS.ActiveNbOfFrames > 0)
            _unit = background.rect.width / _plugin4DS.ActiveNbOfFrames;
        else
            _unit = background.rect.width / _plugin4DS.SequenceNbOfFrames;

        _currentFrame.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _unit*2);
    }


    //Testing: this function will be called when Test Button is clicked
    public void Update()
    {
        _currentFrame.localPosition = new Vector3((_plugin4DS.CurrentFrame-_plugin4DS.FirstActiveFrame) * _unit, 0, 0);

        //-- mesh buffer display
        _meshBuffer.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _unit * _plugin4DS.MeshBufferSize);
        _meshBuffer.localPosition = new Vector3(((_plugin4DS.CurrentFrame - _plugin4DS.FirstActiveFrame) + 1) * _unit, 0, 0);

        float diff = _meshBuffer.localPosition.x + _meshBuffer.rect.size.x - background.rect.size.x;
        if (diff > 0)
        {
            _meshBuffer.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (_unit * _plugin4DS.MeshBufferSize / 2) - diff);

            if (mbDuplicate == null)
                mbDuplicate = Instantiate<RectTransform>(_meshBuffer, this.gameObject.transform);
            mbDuplicate.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, diff);
            mbDuplicate.localPosition = new Vector3(0, 0, 0);
        }
        else
            if (mbDuplicate != null)
                Destroy(mbDuplicate.gameObject);


        //-- chunk buffer display
        _chunkBuffer.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _unit * _plugin4DS.ChunkBufferSize/2);
        _chunkBuffer.localPosition = new Vector3(_meshBuffer.localPosition.x + (_plugin4DS.MeshBufferSize*_unit ), 0, 0);

        diff = _chunkBuffer.localPosition.x + _chunkBuffer.rect.size.x - background.rect.size.x;
        if (diff > 0)
        {
            _chunkBuffer.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (_unit * _plugin4DS.ChunkBufferSize / 2) - diff);

            if (cbDuplicate == null)
                cbDuplicate = Instantiate<RectTransform>(_chunkBuffer, this.gameObject.transform);
            cbDuplicate.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, diff);
            cbDuplicate.localPosition = new Vector3(0, 0, 0);
        }
        else
            if (cbDuplicate != null)
                Destroy(cbDuplicate.gameObject);
    }


    public void onClick()
    {
        if (_unit == 0)
            return;

        //todo: manage mobile touch instead of mouse;
        int clickedFrame;

        if (Input.touchCount == 1){
            Touch touch = Input.GetTouch(0);
            clickedFrame = (int)(touch.position.x / _unit);
        } else {
            clickedFrame = (int)(Input.mousePosition.x / _unit);
        }

        _plugin4DS.GotoFrame(clickedFrame);
    }
}

