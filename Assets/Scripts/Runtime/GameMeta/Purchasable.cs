using System;
using UnityEngine;

namespace Zombies.Runtime.GameMeta
{
    public class Purchasable : MonoBehaviour
    {
        public string purchaseAction = "Purchase Undef";
        public int cost = 100;
        public bool disableOnPurchase = true;

        public event Action PurchaseEvent;
        
        public string display => $"{purchaseAction} [{cost:G} points]";

        public void Purchase()
        {
            PurchaseEvent?.Invoke();
            if (disableOnPurchase) enabled = false;
        }
    }
}