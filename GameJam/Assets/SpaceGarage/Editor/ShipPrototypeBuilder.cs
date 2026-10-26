using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceGarage.Editor
{
    public static class ShipPrototypeBuilder
    {
        private const string Root = "Assets/SpaceGarage/Prototype";

        [MenuItem("Space Garage/Create Control Prototype")]
        public static void Create()
        {
            // Never overwrite an existing prototype or the user's current scene.
            Directory.CreateDirectory(Root);
            string folder = AssetDatabase.GenerateUniqueAssetPath(Root + "/ControlDemo");
            Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
            var previous = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            try
            {
                var gunStats = MakePart(folder, "Gun", ShipPartSlot.Gun);
                var armorStats = MakePart(folder, "ArmorPlate", ShipPartSlot.ArmorPlate);
                var wingStats = MakePart(folder, "Wing", ShipPartSlot.Wing);
                var jetStats = MakePart(folder, "Jet", ShipPartSlot.Jet);

                var bulletObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bulletObject.name = "Bullet";
                bulletObject.transform.localScale = Vector3.one * 0.24f;
                Object.DestroyImmediate(bulletObject.GetComponent<Collider>());
                var bullet = bulletObject.AddComponent<ShipProjectile>();
                var bulletPrefab = PrefabUtility.SaveAsPrefabAsset(bulletObject, folder + "/Bullet.prefab");
                Object.DestroyImmediate(bulletObject);

                var ship = new GameObject("PlayerShip");
                ship.AddComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
                ship.AddComponent<BoxCollider>().size = new Vector3(1, 0.5f, 2);
                ship.AddComponent<ShipInventory>();
                var gunObject = new GameObject("Gun");
                gunObject.transform.SetParent(ship.transform, false);
                var gun = gunObject.AddComponent<ShipGun>();
                var muzzle = new GameObject("Muzzle").transform;
                muzzle.SetParent(gunObject.transform, false);
                muzzle.localPosition = new Vector3(0, 0, 1.3f);
                var gunSerialized = new SerializedObject(gun);
                gunSerialized.FindProperty("projectilePrefabObject").objectReferenceValue = bulletPrefab;
                gunSerialized.FindProperty("muzzle").objectReferenceValue = muzzle;
                gunSerialized.ApplyModifiedPropertiesWithoutUndo();
                gunStats.partPrefab = PrefabUtility.SaveAsPrefabAsset(gunObject, folder + "/GunPart.prefab");
                Object.DestroyImmediate(gunObject);
                MakeEmptyPart(folder, armorStats);
                MakeEmptyPart(folder, wingStats);
                MakeEmptyPart(folder, jetStats);
                var controller = ship.AddComponent<ShipController>();
                controller.SetLoadout(gunStats, armorStats, wingStats, jetStats);
                var controllerSerialized = new SerializedObject(controller);
                foreach (string mountName in new[] { "gunMount", "armorPlateMount", "wingMount", "jetMount" })
                {
                    var mount = new GameObject(mountName).transform;
                    mount.SetParent(ship.transform, false);
                    controllerSerialized.FindProperty(mountName).objectReferenceValue = mount;
                }
                controllerSerialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(gunStats);
                PrefabUtility.SaveAsPrefabAsset(ship, folder + "/PlayerShip.prefab");

                var pickupObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                pickupObject.name = "ScrapPickup";
                pickupObject.transform.localScale = Vector3.one * 0.4f;
                pickupObject.transform.position = new Vector3(2, 0, 2);
                pickupObject.GetComponent<SphereCollider>().isTrigger = true;
                pickupObject.AddComponent<ShipPickup>();
                PrefabUtility.SaveAsPrefabAsset(pickupObject, folder + "/ScrapPickup.prefab");

                var cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                var camera = cameraObject.AddComponent<Camera>();
                camera.orthographic = true; camera.orthographicSize = 12;
                camera.transform.SetPositionAndRotation(new Vector3(0, 25, 0), Quaternion.Euler(90, 0, 0));
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.04f, 0.05f, 0.08f);
                cameraObject.AddComponent<AudioListener>();
                var light = new GameObject("Directional Light").AddComponent<Light>();
                light.type = LightType.Directional;
                light.transform.rotation = Quaternion.Euler(50, -30, 0);

                string scenePath = folder + "/ControlDemo.unity";
                EditorSceneManager.SaveScene(scene, scenePath);
                AssetDatabase.SaveAssets();
                Debug.Log($"Created {scenePath}. Open it to test. PlayerShip intentionally has no renderer; add your model as a child facing local +Z. Select the ship and enable Gizmos to inspect its collider and pickup radius.");
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            }
            finally
            {
                if (previous.IsValid()) SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static ShipPartStats MakePart(string folder, string name, ShipPartSlot slot)
        {
            var asset = ScriptableObject.CreateInstance<ShipPartStats>();
            asset.slot = slot;
            AssetDatabase.CreateAsset(asset, folder + "/" + name + ".asset");
            return asset;
        }

        private static void MakeEmptyPart(string folder, ShipPartStats part)
        {
            var placeholder = new GameObject(part.slot.ToString());
            part.partPrefab = PrefabUtility.SaveAsPrefabAsset(placeholder, folder + "/" + part.slot + "Part.prefab");
            EditorUtility.SetDirty(part);
            Object.DestroyImmediate(placeholder);
        }
    }
}
