using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Flower : MonoBehaviour
{
    [Tooltip("Colour when the flower is full")]
    public Color fullFlowerColor = new Color(1f, 0f, 0.3f);
    [Tooltip("Colour when the flower is empty")]
    public Color emptyFlowerColor = new Color(0.5f, 0f, 1f);
    public Collider nectarCollider;
    public Collider flowerCollider;
    public Material flowerMaterial;
    public Vector3 FlowerUpVector
    {
        get 
        {
            return flowerCollider.transform.up;
        }
    }
    public Vector3 FlowerCenterPosition
    {
        get
        {
            return nectarCollider.transform.position;
        }
    }
    public float NectarAmount { get; private set; }
    public bool HasNectar
    {
        get
        {
            return NectarAmount> 0f;
        }
    }
    public float Feed(float amount)
    {
        float nectarTaken = Mathf.Clamp(amount, 0f, NectarAmount);
        NectarAmount -= amount;
        if (NectarAmount <= 0)
        {
            NectarAmount = 0;
            flowerCollider.gameObject.SetActive(false);
            nectarCollider.gameObject.SetActive(false);
            flowerMaterial.SetColor("_BaseColor", emptyFlowerColor);
        }
        return nectarTaken;
    }
    public void ResetFlower()
    {
        NectarAmount = 1f;
        flowerCollider.gameObject.SetActive(true);
        nectarCollider.gameObject.SetActive(true);
        flowerMaterial.SetColor("BaseColor", fullFlowerColor);
    }
    private void Awake()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        flowerMaterial = meshRenderer.material;
        flowerCollider = transform.Find("FlowerCollider").GetComponent<Collider>();
        nectarCollider = transform.Find("FlowerNectarCollider").GetComponent<Collider>();
    }
}
