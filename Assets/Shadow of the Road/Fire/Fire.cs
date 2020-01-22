using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    public Light PointLight;
    float intensity;

    void Start()
    {
        StartCoroutine(SetIntensity());
    }

    // Update is called once per frame
    void Update()
    {
        PointLight.intensity = Mathf.Lerp(PointLight.intensity, intensity, 0.1f);
    }

    IEnumerator SetIntensity()
    {
        while(true)
        {
            intensity = Random.Range(10, 50);
            yield return new WaitForSeconds(Random.Range(0.1f, 0.2f));

        }
    }
}
