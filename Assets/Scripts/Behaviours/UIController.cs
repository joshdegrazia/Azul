using Azul.Components;
using Unity.Entities;
using UnityEngine;

namespace Azul.Behaviours {
    public class UIController : MonoBehaviour {
        private EntityManager EntityManager;

        private static UIController Instance;

        private void Awake() {
            this.InitializeSingleton();

            this.EntityManager = World.Active.EntityManager;
        }

        private void InitializeSingleton() {
            if (UIController.Instance != null && UIController.Instance != this) {
                Debug.LogError("A second UIController has been created!");
                GameObject.Destroy(this.gameObject);
            }

            UIController.Instance = this;
        }

        public void OnDealTilesFromBagButtonClicked() {
            this.EntityManager.CreateEntity(typeof(PrepareNewRound));
        }
    }
}
