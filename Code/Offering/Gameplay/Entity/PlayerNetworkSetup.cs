using Unity.Netcode;
using UnityEngine;

public class PlayerNetworkSetup : NetworkBehaviour
{
    private Player _player;

    private void Awake()
    {
        _player = GetComponent<Player>();
    }

    public override void OnNetworkSpawn()
    {
        if (_player == null) return;

        bool isLocal = IsOwner;

        _player.StartSetting(OwnerClientId.ToString(), isLocal);
        _player.SetLocalPlayer(isLocal);

        if (isLocal)
        {
            Bus<SwapTrackingEvent>.Raise(
                new SwapTrackingEvent().Initialize(_player.transform)
            );
        }
    }

    public override void OnNetworkDespawn()
    {
        if (_player == null) return;

        _player.SetLocalPlayer(false);
    }
}
