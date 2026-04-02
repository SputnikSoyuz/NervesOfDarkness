using System;

namespace NervesOfDarkness;
public class MediaDiscSocket : OWItemSocket
{
    public override void Awake()
    {
        base.Awake();
        _acceptableType = ItemType.Scroll;
    }

    public override void Start()
    {
        base.Start();
        OnSocketableDonePlacing += OnScrollPlaced;

        if (_socketedItem != null)
        {
            OnScrollPlaced(_socketedItem);
        }
    }

    public void OnDestroy()
    {
        OnSocketableDonePlacing -= OnScrollPlaced;
    }

    public override OWItem RemoveFromSocket()
    {
        NervesOfDarkness.WriteLine("Removed from socket.", OWML.Common.MessageType.Success);
        ((MediaDiscItem)_socketedItem)?.StopAudio();
        return base.RemoveFromSocket();
    }

    private void OnScrollPlaced(OWItem socketable)
    {
        NervesOfDarkness.WriteLine("Socketed Item: " + _socketedItem.ToString() +
                                    "\n Socketable: " + socketable.ToString() +
                                    "\n Equal? : " + _socketedItem.Equals(socketable), OWML.Common.MessageType.Success);
        ((MediaDiscItem)socketable)?.PlayAudio();
    }
}
