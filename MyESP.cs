using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CustomESP
{
    public class MyESP : MonoBehaviour
    {
        private Camera mainCamera;
        private List<GameObject> enemies = new List<GameObject>();
        private float scaleX, scaleY;
        private static Texture2D texture2;

        void Start()
        {
            mainCamera = Camera.main;
            if (texture2 == null)
            {
                texture2 = new Texture2D(1, 1);
                texture2.SetPixel(0, 0, Color.white);
                texture2.Apply();
            }

            UpdateEnemyList(); // Initial enemy scan
            InvokeRepeating(nameof(UpdateEnemyList), 2f, 2f); // Re-scan every 2 seconds
        }

        void Update()
        {
            if (mainCamera == null || mainCamera != Camera.main)
            {
                mainCamera = Camera.main;
            }

            scaleX = (float)Screen.width / mainCamera.pixelWidth;
            scaleY = (float)Screen.height / mainCamera.pixelHeight;
        }

        void OnGUI()
        {
            if (mainCamera == null)
            {
                GUI.Label(new Rect(10, 10, 300, 20), "ESP ERROR: No camera detected!");
                return;
            }

            GUI.Label(new Rect(10, 30, 300, 20), $"ESP DEBUG: Found {enemies.Count} enemies");

            foreach (GameObject enemy in enemies)
            {
                if (enemy == null || !enemy.activeInHierarchy) continue;

                string enemyName = GetEnemyName(enemy);
                Bounds enemyBounds = GetFixedColliderBounds(enemy);
                if (enemyBounds.size == Vector3.zero) continue;

                Vector3 basePosition = enemyBounds.center;
                float enemyHeight = enemyBounds.extents.y * 2;
                float enemyWidth = enemyBounds.extents.x * 2;

                // Special case for Peeper (if missing colliders)
                if (enemyName.ToLower().Contains("peeper"))
                {
                    basePosition = GetPeeperPosition(enemy);
                    enemyHeight = 1.5f;
                }

                Vector3 footPosition = basePosition - new Vector3(0, enemyHeight / 2, 0);
                Vector3 headPosition = basePosition + new Vector3(0, enemyHeight / 2, 0);

                Vector3 screenFootPos = mainCamera.WorldToScreenPoint(footPosition);
                Vector3 screenHeadPos = mainCamera.WorldToScreenPoint(headPosition);

                if (screenFootPos.z > 0 && screenHeadPos.z > 0)
                {
                    float footX = screenFootPos.x * scaleX;
                    float footY = Screen.height - (screenFootPos.y * scaleY);
                    float headY = Screen.height - (screenHeadPos.y * scaleY);

                    float height = Mathf.Abs(footY - headY);
                    float distance = screenFootPos.z;

                    float baseWidth = enemyWidth * 200f;
                    float width = (baseWidth / distance) * scaleX;
                    width = Mathf.Clamp(width, 30f, height * 1.2f);
                    height = Mathf.Clamp(height, 40f, 400f);

                    float x = footX - width / 2;
                    float y = headY;

                    DrawBoxOutline(x, y, width, height, Color.red);
                    DrawTracerLine(footX, footY, Color.red);

                    // Display enemy name + distance at head level, in white
                    GUI.color = Color.white;
                    string label = $"{enemyName} [{distance:F1}m]";
                    GUI.Label(new Rect(x + width / 2 - 30, y - 15, 150, 20), label);
                    GUI.color = Color.white; // Reset color
                }
            }
        }

        private void UpdateEnemyList()
        {
            enemies.Clear();

            Type enemyDirectorType = Type.GetType("EnemyDirector, Assembly-CSharp");
            if (enemyDirectorType != null)
            {
                object enemyDirectorInstance = enemyDirectorType.GetField("instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
                if (enemyDirectorInstance != null)
                {
                    FieldInfo enemiesSpawnedField = enemyDirectorType.GetField("enemiesSpawned", BindingFlags.Public | BindingFlags.Instance);
                    if (enemiesSpawnedField != null)
                    {
                        var enemiesFound = enemiesSpawnedField.GetValue(enemyDirectorInstance) as IEnumerable<object>;
                        if (enemiesFound != null)
                        {
                            foreach (var enemy in enemiesFound)
                            {
                                if (enemy != null)
                                {
                                    FieldInfo enemyInstanceField = enemy.GetType().GetField("enemyInstance", BindingFlags.NonPublic | BindingFlags.Instance)
                                                              ?? enemy.GetType().GetField("Enemy", BindingFlags.NonPublic | BindingFlags.Instance)
                                                              ?? enemy.GetType().GetField("childEnemy", BindingFlags.NonPublic | BindingFlags.Instance);
                                    if (enemyInstanceField != null)
                                    {
                                        var enemyInstance = enemyInstanceField.GetValue(enemy) as MonoBehaviour;
                                        if (enemyInstance != null && enemyInstance.gameObject != null && enemyInstance.gameObject.activeInHierarchy)
                                        {
                                            enemies.Add(enemyInstance.gameObject);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static Bounds GetFixedColliderBounds(GameObject obj)
        {
            Bounds bounds = GetActiveColliderBounds(obj);
            if (bounds.size != Vector3.zero) return bounds;

            if (obj.transform.parent != null)
            {
                bounds = GetActiveColliderBounds(obj.transform.parent.gameObject);
                if (bounds.size != Vector3.zero) return bounds;
            }

            return new Bounds(obj.transform.position, new Vector3(1, 2, 1));
        }

        private static Bounds GetActiveColliderBounds(GameObject obj)
        {
            Collider[] colliders = obj.GetComponentsInChildren<Collider>(true);
            List<Collider> activeColliders = new List<Collider>();

            foreach (Collider col in colliders)
            {
                if (col.enabled && col.gameObject.activeInHierarchy)
                    activeColliders.Add(col);
            }

            if (activeColliders.Count == 0)
            {
                Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length > 0)
                {
                    Bounds bounds = renderers[0].bounds;
                    for (int i = 1; i < renderers.Length; i++)
                    {
                        if (renderers[i].enabled && renderers[i].gameObject.activeInHierarchy)
                            bounds.Encapsulate(renderers[i].bounds);
                    }
                    return bounds;
                }
                return new Bounds(obj.transform.position, Vector3.one * 0.5f);
            }

            Bounds resultBounds = activeColliders[0].bounds;
            for (int i = 1; i < activeColliders.Count; i++)
            {
                resultBounds.Encapsulate(activeColliders[i].bounds);
            }
            resultBounds.Expand(0.1f);
            return resultBounds;
        }

        private static string GetEnemyName(GameObject enemy)
        {
            var enemyParent = enemy.GetComponentInParent(Type.GetType("EnemyParent, Assembly-CSharp"));
            if (enemyParent != null)
            {
                var nameField = enemyParent.GetType().GetField("enemyName", BindingFlags.Public | BindingFlags.Instance);
                if (nameField != null)
                {
                    string foundName = nameField.GetValue(enemyParent)?.ToString();
                    if (!string.IsNullOrEmpty(foundName)) return foundName;
                }
            }

            return enemy.transform.parent != null ? enemy.transform.parent.name : enemy.name;
        }

        private static Vector3 GetPeeperPosition(GameObject enemy)
        {
            return enemy.transform.position + Vector3.up * 1.5f;
        }

        private void DrawTracerLine(float x, float y, Color color)
        {
            DrawLine(new Vector2(x, y), new Vector2(Screen.width / 2, Screen.height), color);
        }

        private void DrawBoxOutline(float x, float y, float w, float h, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(new Rect(x, y, w, 2f), texture2);
            GUI.DrawTexture(new Rect(x, y + h - 2f, w, 2f), texture2);
            GUI.DrawTexture(new Rect(x, y, 2f, h), texture2);
            GUI.DrawTexture(new Rect(x + w - 2f, y, 2f, h), texture2);
        }
        private void DrawLine(Vector2 start, Vector2 end, Color color)
        {
            if (texture2 == null) return;

            float distance = Vector2.Distance(start, end);
            float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;

            GUI.color = color;
            Matrix4x4 originalMatrix = GUI.matrix;
            GUIUtility.RotateAroundPivot(angle, start);
            GUI.DrawTexture(new Rect(start.x, start.y, distance, 1f), texture2);
            GUI.matrix = originalMatrix;
            GUI.color = Color.white; // Reset GUI color after drawing
        }

    }
}
