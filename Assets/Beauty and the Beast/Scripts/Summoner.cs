using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;

public class Summoner : MonoBehaviour
{
    public GameObject Beauty;
    public GameObject Beast;
    public GameObject VFX;
    private GameObject VFXInstance;
    public Light Light;
    public Texture2D LightColor;

    private Texture2D pointCache;
    private float size;

    private bool summonning;

    void Start()
    {
        this.summonning = false;
        this.Beast.transform.position = this.Beauty.transform.position;
        this.Beast.transform.rotation = this.Beauty.transform.rotation;
        this.Beast.SetActive(false);
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
        float lightStep = 0;
        this.summonning = true;
        float minClippingLevel = -1;
        float maxClippingLevel = 2;
        float clippingLevel = maxClippingLevel;
        this.Beauty.SetActive(true);
        this.VFXInstance = Instantiate(this.VFX);
        this.VFXInstance.transform.position = this.Beauty.transform.position;
        this.VFXInstance.transform.rotation = this.Beauty.transform.rotation;
        this.Light.transform.position = this.Beauty.transform.position + new Vector3(0, 3, 0);
        while (clippingLevel > minClippingLevel)
        {
            this.UpdateSize(this.Beauty);
            this.UpdateCachePoint(this.Beauty);
            clippingLevel -= Mathf.Abs(maxClippingLevel - minClippingLevel) / 5 * Time.deltaTime;
            lightStep = Mathf.Abs(maxClippingLevel - clippingLevel) / Mathf.Abs(maxClippingLevel - minClippingLevel) * 0.5f;
            SkinnedMeshRenderer[] renderers = this.Beauty.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    material.SetFloat("_ClippingLevel", clippingLevel);
                }

            }
            this.VFXInstance.GetComponent<VisualEffect>().SetTexture("PointCache", this.pointCache);
            this.VFXInstance.GetComponent<VisualEffect>().SetFloat("Size", this.size);
            this.VFXInstance.GetComponent<VisualEffect>().SetFloat("ClippingLevel", clippingLevel - 0.5f);
            this.VFXInstance.GetComponent<VisualEffect>().SetBool("Emit", true);
            this.Light.color = LightColor.GetPixel((int)(lightStep * LightColor.width), (int)(0.5f * LightColor.width));
            yield return 0;
        }
        this.Beauty.SetActive(false);
        yield return new WaitForSeconds(3);
        minClippingLevel = -1;
        maxClippingLevel = 3;
        this.Beast.SetActive(true);
        while (clippingLevel < maxClippingLevel)
        {
            this.UpdateSize(this.Beast);
            this.UpdateCachePoint(this.Beast);
            clippingLevel += Mathf.Abs(maxClippingLevel - minClippingLevel) / 10 * Time.deltaTime;
            lightStep = (1 - Mathf.Abs(maxClippingLevel - clippingLevel) / Mathf.Abs(maxClippingLevel - minClippingLevel)) * 0.5f + 0.5f;
            SkinnedMeshRenderer[] renderers = this.Beast.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    material.SetFloat("_ClippingLevel", clippingLevel);
                }
            }
            this.VFXInstance.GetComponent<VisualEffect>().SetTexture("PointCache", this.pointCache);
            this.VFXInstance.GetComponent<VisualEffect>().SetFloat("Size", this.size);
            this.VFXInstance.GetComponent<VisualEffect>().SetFloat("ClippingLevel", clippingLevel);
            this.VFXInstance.GetComponent<VisualEffect>().SetBool("Emit", false);
            this.Light.color = LightColor.GetPixel((int)(lightStep * LightColor.width), (int)(0.5f * LightColor.width));
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
