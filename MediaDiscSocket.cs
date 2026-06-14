using System;
using System.Collections;
using UnityEngine;

namespace NervesOfDarkness;
public class MediaDiscSocket : OWItemSocket
{
    [SerializeField]
    private Animator animator;

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
        animator.Play("SpeakerOff", 0);
        ((MediaDiscItem)_socketedItem)?.StopAudio();
        NervesOfDarkness.WriteLine("Removed from socket.", OWML.Common.MessageType.Success);
        return base.RemoveFromSocket();
    }

    private void OnScrollPlaced(OWItem socketable)
    {
        animator.Play("SpeakerOn", 0);
        NervesOfDarkness.WriteLine("Socketed Item: " + _socketedItem.ToString() +
                                    "\n Socketable: " + socketable.ToString() +
                                    "\n Equal? : " + _socketedItem.Equals(socketable), OWML.Common.MessageType.Success);
        ((MediaDiscItem)socketable)?.PlayAudio();
    }
}
