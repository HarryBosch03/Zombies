using System.Collections.Generic;
using UnityEngine;

namespace Framework.Runtime.Rendering
{
    [ExecuteAlways]
    [SelectionBase, DisallowMultipleComponent]
    public sealed class ThickSprite : MonoBehaviour
    {
        public const float ppu = 16.0f;
        public const float scale = 1 / 3.0f;
        public const float seccondsPerRevolution = 4.0f;
        public const float bobFrequency = 4.0f;
        public const float bobAmplitude = 0.2f;

        public Texture2D texture;
        public Material material;

        private Texture2D generatedTexture;
        private Mesh mesh;

        private void OnEnable()
        {
            mesh = new Mesh();
            mesh.hideFlags = HideFlags.HideAndDontSave;
            
            GenerateMesh();
        }

        private void OnDisable() { DestroyImmediate(mesh); }

        private void Update()
        {
            if (texture != generatedTexture) GenerateMesh();

            var rotationSpeed = 360.0f / seccondsPerRevolution;
            var bob = Mathf.Sin(Time.time * rotationSpeed * Mathf.Deg2Rad * bobFrequency) * bobAmplitude * scale;
            Graphics.DrawMesh(mesh, transform.position + Vector3.up * bob, Quaternion.Euler(0.0f, Time.time * rotationSpeed, 0.0f) * transform.rotation, material, gameObject.layer);
        }

        private void GenerateMesh()
        {
            generatedTexture = texture;
            
            if (!texture || !texture.isReadable)
            {
                mesh.Clear();
                return;
            }

            var vertices = new List<Vector3>();
            var colors = new List<Color>();
            var indices = new List<int>();

            for (var x = 0; x < texture.width; x++)
            for (var y = 0; y < texture.height; y++)
            {
                var c = texture.GetPixel(x, y);
                if (c.a * 255 < 1) continue;

                addCube(new Vector3(x - texture.width / 2.0f, y - texture.height / 2.0f, 0.0f), c);
            }

            void addCube(Vector3 offset, Color color)
            {
                addQuad(Vector3.right, Vector3.forward, offset, color);
                
                addQuad(Vector3.right, Vector3.up, offset, color);
                addQuad(Vector3.forward, Vector3.up, offset, color);
                addQuad(Vector3.left, Vector3.up, offset, color);
                addQuad(Vector3.back, Vector3.up, offset, color);
                
                addQuad(Vector3.right, Vector3.back, offset, color);
            }
            
            void addQuad(Vector3 tangent, Vector3 bitangent, Vector3 offset, Color color)
            {
                var normal = Vector3.Cross(tangent, bitangent);

                var start = vertices.Count;
                vertices.Add((offset + (-tangent - bitangent + normal) * 0.5f) / ppu * scale);
                vertices.Add((offset + (tangent - bitangent + normal) * 0.5f) / ppu * scale);
                vertices.Add((offset + (tangent + bitangent + normal) * 0.5f) / ppu * scale);
                vertices.Add((offset + (-tangent + bitangent + normal) * 0.5f) / ppu * scale);

                var exponent = 2.2f;
                var rawColor = new Color(Mathf.Pow(color.r, exponent), Mathf.Pow(color.g, exponent), Mathf.Pow(color.b, exponent), color.a);
                
                colors.Add(rawColor);
                colors.Add(rawColor);
                colors.Add(rawColor);
                colors.Add(rawColor);

                indices.Add(start + 0);
                indices.Add(start + 1);
                indices.Add(start + 2);
                indices.Add(start + 2);
                indices.Add(start + 3);
                indices.Add(start + 0);
            }

            mesh.Clear();
            mesh.vertices = vertices.ToArray();
            mesh.colors = colors.ToArray();
            mesh.triangles = indices.ToArray();
            mesh.RecalculateNormals();
        }
    }
}