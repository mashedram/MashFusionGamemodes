using Il2CppSLZ.Combat;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.AI;
using Il2CppSLZ.Marrow.Combat;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Extensions;
using MashGamemodeLibrary.Entities.Association.Impl;
using MashGamemodeLibrary.Entities.Behaviour;
using MashGamemodeLibrary.Entities.Behaviour.Cache;
using MashGamemodeLibrary.Util;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MashGamemodeLibrary.Entities.Interaction.Attacking;

[RequireStaticConstructor]
public static class AttackManager
{
    private static readonly HashSet<GameObject> CustomAttackReceivers = new(new UnityComparer());
    private static readonly IAssociatedBehaviourCache<NetworkEntityAssociation, IAttackReceiver> AttackReceiverCache = BehaviourManager.CreateCache<NetworkEntityAssociation, IAttackReceiver>();

    static AttackManager()
    {
        AttackReceiverCache.OnAdded += (association, receiver) =>
        {
            if (!association.NetworkID.TryGetEntity(out var entity))
                return;
            
            var marrow = entity.GetExtender<IMarrowEntityExtender>();
            if (marrow == null)
                return;
            
            var go = marrow.MarrowEntity.gameObject;
            // The entity has the attack receiver, we're done
            if (go.GetComponent<GenericAttackReceiver>() != null)
                return;

            go.AddComponent<GenericAttackReceiver>();
            CustomAttackReceivers.Add(go);
        };
        
        AttackReceiverCache.OnRemoved += (association, receiver) =>
        {
            if (!association.NetworkID.TryGetEntity(out var entity))
                return;
            
            var marrow = entity.GetExtender<IMarrowEntityExtender>();
            if (marrow == null)
                return;
            
            var go = marrow.MarrowEntity.gameObject;
            if (!CustomAttackReceivers.Remove(go))
                return;

            var attackReceiver = go.GetComponent<GenericAttackReceiver>();
            if (attackReceiver == null)
                return;
            
            Object.Destroy(attackReceiver);
        };
    }
    
    public static void ReceiveAttack(GenericAttackReceiver receiver, Attack attack)
    {
        if (!MarrowEntity.Cache.TryGet(receiver.gameObject, out var marrow))
            return;

        if (!IMarrowEntityExtender.Cache.TryGet(marrow, out var extender))
            return;

        var proxy = attack.proxy;
        if (proxy.triggerType != TriggerRefProxy.TriggerType.Player)
            return;
        
        if (!RigManager.Cache.TryGet(proxy.root, out var rm))
            return;

        if (!NetworkPlayerManager.TryGetPlayer(rm, out var player))
            return;
        
        var attachReceivers = AttackReceiverCache.GetAll(extender.ID);
        foreach (var attachReceiver in attachReceivers)
        {
            attachReceiver.ReceiveAttack(attack.damage, player);
        }
    }
}