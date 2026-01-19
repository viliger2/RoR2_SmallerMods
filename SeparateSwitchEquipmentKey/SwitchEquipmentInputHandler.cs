using Rebindables;
using RoR2;
using UnityEngine;

namespace SeparateSwitchEquipmentKey
{
    public class SwitchEquipmentInputHandler : MonoBehaviour
    {
        public EquipmentSlot equipmentSlot;

        private void Awake()
        {
            if (!equipmentSlot)
            {
                equipmentSlot = GetComponent<EquipmentSlot>();
            }
        }

        private void FixedUpdate()
        {
            if (!equipmentSlot || !equipmentSlot.inventory || !equipmentSlot.inputBank)
            {
                return;
            }

            if (!(equipmentSlot.hasEffectiveAuthority && equipmentSlot.characterBody && equipmentSlot.characterBody.isEquipmentActivationAllowed))
            {
                return;
            }

            if (equipmentSlot.inventory.GetItemCountEffective(DLC3Content.Items.ExtraEquipment) > 0 && equipmentSlot.inputBank.GetButtonState(SeparateSwitchEquipmentKeyPlugin.SwitchEquipment).justPressed)
            {
                equipmentSlot.inventory.DispatchSwitchToNextEquipmentInSet();
            }
        }

    }
}
