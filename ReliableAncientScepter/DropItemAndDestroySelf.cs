using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using UnityEngine;
using RoR2;

namespace ReliableAncientScepter
{
    public class DropItem : MonoBehaviour
    {
        public ItemDef itemToDrop;

        public Transform dropletOrigin;

        public void DropAndDestroySelf(Interactor interactor)
        {
            if (!NetworkServer.active)
            {
                return;
            }

            if (!itemToDrop)
            {
                return;
            }

            var spawnPoint = dropletOrigin;
            if (!spawnPoint)
            {
                spawnPoint = transform;
            }

            var vector = Vector3.up * 20f + transform.forward * 2f;
            PickupDropletController.CreatePickupDroplet(new UniquePickup(PickupCatalog.FindPickupIndex(itemToDrop.itemIndex)), spawnPoint.position, vector, false);
            NetworkServer.Destroy(this.gameObject);
        }

    }
}
