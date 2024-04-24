using System.Collections.Generic;
using UnityEngine;

namespace Arys.BetterInteractions.Components
{
    public class BetterInteractionsManager : MonoBehaviour
    {
        public static BetterInteractionsManager Instance { get; private set; }
        public BetterInteractionsOutline CachedOutlineComponent { get; set; } = null;
        public Collider[] CachedDetectedColliders { get; private set; } = new Collider[30];
        public HashSet<BetterInteractionsPhysicsDoor> CachedPhysicsDoors { get; private set; } = [];

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            CachedOutlineComponent = null;
            CachedDetectedColliders = null;
            CachedPhysicsDoors = null;
        }
    }
}
