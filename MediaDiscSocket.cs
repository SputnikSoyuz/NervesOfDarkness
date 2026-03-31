using System;

namespace NervesOfDarkness;
public class MediaDiscSocket : OWItemSocket
{
    private MediaDiscItem _mediaDisc;
    public override void Awake()
    {
        base.Awake();
        _acceptableType = ItemType.Scroll;
    }

    public override void Start()
    {
        if (_socketedItem != null)
        {
            base.Start();
            if (_socketedItem.GetType() == typeof(MediaDiscItem))
            {
                if (_socketedItem != null)
                {
                    MediaDiscItem scrollItem = _socketedItem as MediaDiscItem;
                }
                OnSocketableDonePlacing = (SocketEvent)Delegate.Combine(OnSocketableDonePlacing, new SocketEvent(OnScrollPlaced));
                _mediaDisc = _socketedItem.GetComponent<MediaDiscItem>();
            }
        }
    }

    private void OnDestroy()
    {
        OnSocketableDonePlacing = (SocketEvent)Delegate.Remove(OnSocketableDonePlacing, new SocketEvent(OnScrollPlaced));
    }

    public override OWItem RemoveFromSocket()
    {
        _mediaDisc.StopAudio();
        return base.RemoveFromSocket();
    }

    private void OnScrollPlaced(OWItem socketable)
    {
        _mediaDisc.PlayAudio();
    }
}
