using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Summoner : MonoBehaviour
{
    public GameObject Beauty;
    public GameObject Beast;
    public GameObject VFX;

    private Texture2D pointCache;
    private float size;

    private bool summonning;

    void Start()
    {
        this.summonning = false;
    }
    
    void Update()
    {
        if(!this.summonning && Input.GetKeyDown(KeyCode.Space) == true)
        {
            StartCoroutine(Summon());
        }
    }

    private IEnumerator Summon()
    {
        this.summonning = true;
        float minClippingLevel = 0;
        float maxClippingLevel = 2;
        float clippingLevel = maxClippingLevel;
        while (clippingLevel > minClippingLevel)
        {
            this.UpdateSize(this.Beauty);
            this.UpdateCachePoint(this.Beauty);
            clippingLevel -= Mathf.Abs(maxClippingLevel - minClippingLevel) / 2 * Time.deltaTime;
            SkinnedMeshRenderer[] renderers = this.Beauty.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    material.SetFloat("_ClippingLevel", clippingLevel);
                }
            }
            yield return 0;
        }
        yield return new WaitForSeconds(1);
        minClippingLevel = 0;
        maxClippingLevel = 3;
        while (clippingLevel < maxClippingLevel)
        {
            this.UpdateSize(this.Beast);
            this.UpdateCachePoint(this.Beast);
            clippingLevel += Mathf.Abs(maxClippingLevel - minClippingLevel) / 2 * Time.deltaTime;
            SkinnedMeshRenderer[] renderers = this.Beast.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    material.SetFloat("_ClippingLevel", clippingLevel);
                }
            }
            yield return 0;
        }
        yield return new WaitForSeconds(1);
        this.summonning = false;
    }

    void UpdateSize(GameObject character)
    {
        SkinnedMeshRenderer[] renderers = character.GetComponentsInChildren<SkinnedMeshRenderer>();
        Bounds bound = new Bounds();
        foreach(SkinnedMeshRenderer renderer in renderers)
        {
            Mesh baked = new Mesh();
            renderer.BakeMesh(baked);
            bound.Encapsulate(baked.bounds);
        }
        this.size = Mathf.Max(bound.extents.x * 2, bound.extents.y * 2, bound.extents.z * 2);
    }

    void UpdateCachePoint(GameObject character)
    {
        Mesh baked;
        Vector3[] vertices;
        Transform parent;
        SkinnedMeshRenderer[] renderers = character.GetComponentsInChildren<SkinnedMeshRenderer>();
        List<Color> normalizedVertices = new List<Color>();
        foreach (SkinnedMeshRenderer renderer in renderers)
        {
            parent = renderer.gameObject.transform.parent;
            baked = new Mesh();
            renderer.BakeMesh(baked);
            vertices = baked.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = (character.gameObject.transform.InverseTransformPoint(renderer.gameObject.transform.TransformPoint(vertices[i])) + new Vector3(size * 0.5f, 0, size * 0.5f)) / size;
                normalizedVertices.Add(new Color(vertices[i].x, vertices[i].y, vertices[i].z));
            }
        }
        if(this.pointCache == null || this.pointCache.width != normalizedVertices.Count)
        {
            this.pointCache = new Texture2D(1, normalizedVertices.Count, TextureFormat.RGBA32, false, true);
            this.pointCache.filterMode = FilterMode.Point;
        }
        this.pointCache.SetPixels(normalizedVertices.ToArray());
        this.pointCache.Apply();
    }
}
