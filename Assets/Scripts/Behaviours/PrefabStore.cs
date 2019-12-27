using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Azul.Behaviours {
    public class PrefabStore : MonoBehaviour {
        [SerializeField] private GameObject BlackTileGameObjectPrefab;
        [SerializeField] private GameObject BlueTileGameObjectPrefab;
        [SerializeField] private GameObject CyanTileGameObjectPrefab;
        [SerializeField] private GameObject RedTileGameObjectPrefab;
        [SerializeField] private GameObject YellowTileGameObjectPrefab;

        public Entity BlackTilePrefab { get; private set; }
        public Entity BlueTilePrefab { get; private set; }
        public Entity CyanTilePrefab { get; private set; }
        public Entity RedTilePrefab { get; private set; }
        public Entity YellowTilePrefab { get; private set; }

        // does not include first-player tile
        public List<Entity> TilePrefabs { get; private set; }

        public static PrefabStore Instance { get; private set; }

        private void Awake() {
            this.InitializeSingleton();
 
            // TODO: Cleanup BlobAssetStore object. This appears to be causing the memory leak errors that appear on build.
            GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, 
                                                                                           new BlobAssetStore());
            this.BlackTilePrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(this.BlackTileGameObjectPrefab, settings);
            this.BlueTilePrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(this.BlueTileGameObjectPrefab, settings);
            this.CyanTilePrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(this.CyanTileGameObjectPrefab, settings);
            this.RedTilePrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(this.RedTileGameObjectPrefab, settings);
            this.YellowTilePrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(this.YellowTileGameObjectPrefab, settings);

            this.TilePrefabs = new List<Entity>();
            this.TilePrefabs.Add(this.BlackTilePrefab);
            this.TilePrefabs.Add(this.BlueTilePrefab);
            this.TilePrefabs.Add(this.CyanTilePrefab);
            this.TilePrefabs.Add(this.RedTilePrefab);
            this.TilePrefabs.Add(this.YellowTilePrefab);
        }

        private void InitializeSingleton() {
            if (PrefabStore.Instance != null && PrefabStore.Instance != this) {
                Debug.LogError("A second PrefabStore has been created!");
                GameObject.Destroy(this.gameObject);
            }

            PrefabStore.Instance = this;
        }
    }
}
