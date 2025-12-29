using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Il2CppSLZ.Marrow.Forklift.Model;
using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Downloading;
using LabFusion.Downloading.ModIO;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Marrow;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.Preferences.Client;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking.Control;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MelonLoader;
using UnityEngine;
using ExecutionContext = MashGamemodeLibrary.Execution.ExecutionContext;
using INetSerializable = LabFusion.Network.Serialization.INetSerializable;

namespace MashGamemodeLibrary.networking.Mods;

internal class ModDownloadRequest : INetSerializable
{
    private int? _fileId;
    private int _modId;

    public ModDownloadRequest()
    {
        _modId = 0;
        _fileId = null;
    }

    public ModDownloadRequest(ModIOModTarget target)
    {
        _modId = (int)target.ModId;
        _fileId = (int?)target.ModfileId;
    }

    public int ModId => _modId;

    public ModIOFile ModFile
    {
        get => new(_modId, _fileId);
        set
        {
            _modId = value.ModID;
            _fileId = value.FileID;
        }
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref _modId);
        serializer.SerializeValue(ref _fileId);
    }
}

internal class ModDownloadResponse : INetSerializable, IKnownSenderPacket
{
    public int ModId;
    public bool Success;

    public ModDownloadResponse()
    {
        ModId = 0;
        Success = false;
    }

    public ModDownloadResponse(int modId, bool success = true)
    {
        ModId = modId;
        Success = success;
    }
    public byte SenderSmallId { get; set; }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref ModId);
        serializer.SerializeValue(ref Success);
    }
}

internal class PalletState
{

    public PalletState(ModDownloadRequest request)
    {
        Request = request;
    }
    public ModDownloadRequest Request { get; }
    public DateTime LastRequestAt { get; set; }
    public HashSet<PlayerID> AwaitingResponseFrom { get; } = new();
    public HashSet<PlayerID> ApprovedBy { get; } = new();

    public void FilterOfflinePlayers()
    {
        var onlinePlayers = NetworkPlayer.Players.Select(p => p.PlayerID);
        AwaitingResponseFrom.RemoveWhere(p => !onlinePlayers.Contains(p));
        ApprovedBy.RemoveWhere(p => !onlinePlayers.Contains(p));
    }

    public IEnumerable<PlayerID> GetUnapprovedUsers()
    {
        return NetworkPlayer.Players.Select(p => p.PlayerID).Where(p => !p.IsHost).Except(ApprovedBy);
    }

    public void Approve(PlayerID playerId)
    {
        // We assume the host has the mod
        if (playerId.IsHost)
            return;

        AwaitingResponseFrom.Remove(playerId);
        ApprovedBy.Add(playerId);
    }

    public bool IsApproved()
    {
        FilterOfflinePlayers();

        if (AwaitingResponseFrom.Count > 0)
            return false;

        var totalPlayerCount = NetworkPlayer.Players.Count;
        // Add one to include host
        return ApprovedBy.Count + 1 >= totalPlayerCount;
    }
}

public static class ModRequirementSyncer
{
    private static readonly RemoteEvent<ModDownloadRequest> ModDownloadRequestEvent = new(OnModDownloadRequest, CommonNetworkRoutes.HostToRemote);
    private static readonly RemoteEvent<ModDownloadResponse> ModDownloadResponseEvent = new(OnModDownloadResponse, CommonNetworkRoutes.RemoteToHost);

    private static readonly Dictionary<int, PalletState> _palletStates = new();

    private static bool TryGetModInfo(Pallet pallet, [MaybeNullWhen(false)] out ModIOModTarget modTarget)
    {
        modTarget = null;
        // TODO: Add to list of approved mods
        if (pallet.IsInMarrowGame())
            return false;

        var manifest = CrateFilterer.GetManifest(pallet);

        // No manifest, just approve it and accept some users may not have it
        if (manifest == null)
            return false;

        var listing = manifest.ModListing;
        modTarget = ModIOManager.GetTargetFromListing(listing);
        return modTarget != null;
    }

    // This is kinda just an edit of labfusions internal system, but I need my own handlers after that
    private static void AddRequiredPallet(Pallet pallet)
    {
        if (!TryGetModInfo(pallet, out var modTarget))
            return;

        var request = new ModDownloadRequest(modTarget);
        _palletStates.TryAdd(request.ModId, new PalletState(request));
    }

    public static void SetPallets(IEnumerable<Pallet> pallets)
    {
        _palletStates.Clear();
        pallets.ForEach(AddRequiredPallet);
    }

    // Pallet Checking

    public static bool CheckApprovals(TimeSpan timeout)
    {
        var now = DateTime.UtcNow;
        return _palletStates.Values.Where(palletState => now - palletState.LastRequestAt <= timeout).All(palletState => palletState.IsApproved());
    }

    // Pallet Approving

    private static void SendApprovalRequest(PalletState state)
    {
        state.LastRequestAt = DateTime.UtcNow;
        var unapprovedUsers = state.GetUnapprovedUsers();
        foreach (var playerID in unapprovedUsers)
        {
            state.AwaitingResponseFrom.Add(playerID);
            ModDownloadRequestEvent.CallFor(playerID, state.Request);
        }
    }

    public static void SendApprovalRequests()
    {
        foreach (var palletStatesValue in _palletStates.Values)
        {
            SendApprovalRequest(palletStatesValue);
        }
    }

    // Mod Downloading

    private static IEnumerator WaitAndInstallMod(ModDownloadRequest downloadRequest)
    {
        var elapsed = 0f;
        var receivedCallback = false;

        var temporary = !ClientSettings.Downloading.KeepDownloadedMods.Value;
        ModIODownloader.EnqueueDownload(new ModTransaction
        {
            ModFile = downloadRequest.ModFile,
            Temporary = temporary,
            Callback = ModDownloadedCallback
        });

        while (!receivedCallback && elapsed < 5f)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        yield break;

        void ModDownloadedCallback(DownloadCallbackInfo info)
        {
            // TODO: Handle download failures

            receivedCallback = true;
            OnModInstalled(downloadRequest.ModId);
        }
    }

    private static void OnModInstalled(int modId)
    {
        ModDownloadResponseEvent.Call(new ModDownloadResponse(modId));
    }

    private static bool HasModInstalled(int modId)
    {
        return AssetWarehouse.Instance.modioPalletManifestsLookup.ContainsKey(modId);
    }

    // Events

    [RunIf(ExecutionContext.Remote)]
    private static void OnModDownloadRequest(ModDownloadRequest packet)
    {
        if (HasModInstalled(packet.ModId))
        {
            OnModInstalled(packet.ModId);
            return;
        }
        MelonCoroutines.Start(WaitAndInstallMod(packet));
    }

    private static void OnModDownloadResponse(ModDownloadResponse packet)
    {
        if (!_palletStates.TryGetValue(packet.ModId, out var state))
            return;

        var playerId = PlayerIDManager.GetPlayerID(packet.SenderSmallId);
        if (playerId == null)
            return;

        state.Approve(playerId);
    }
}