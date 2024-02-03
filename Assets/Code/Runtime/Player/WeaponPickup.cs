using Framework.Runtime.Interactions;
using UnityEngine;

namespace Framework.Runtime.Player
{
    public class WeaponPickup : Interactable
    {
        private object args;
        
        public override float InteractDuration => ShortInteract;

        protected override void OnStartInteract()
        {
            
        }

        protected override void OnEndInteract(bool finished)
        {
            if (!finished) return;
            
            var weaponManager = Interactor.GetComponent<PlayerWeaponManager>();
            if (!weaponManager) return;
            
            weaponManager.EquipWeapon(name, args);
        }
    }
}