using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CertainHallowedConcepts
{
    public class DropSacrificePage : MonoBehaviour
    {
        public ItemDef itemToDrop;

        private WakeController controller;

        private void Awake()
        {
            controller = GetComponent<WakeController>();
        }

        public void DropItemServer()
        {
            if (!NetworkServer.active)
            {
                return;
            }

            if (!itemToDrop)
            {
                return;
            }

            if (!controller || !controller.shamanPickupCreator)
            {
                return;
            }

            if(controller.sacrificesKilled > 0)
            {
                return;
            }

            var spawnPoint = controller.shamanPickupCreator.transform;

            var vector = Vector3.up * controller.shamanPickupCreator.pickupVelocityY + transform.forward * controller.shamanPickupCreator.pickupVelocityZ;
            PickupDropletController.CreatePickupDroplet(new UniquePickup(PickupCatalog.FindPickupIndex(itemToDrop.itemIndex)), spawnPoint.position, vector, false);
        }
    }
}
