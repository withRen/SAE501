using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Nail : MonoBehaviour
{
    public Plank plank;
    public GameObject visual;
    public Collider col;
    public int hitsNeeded = 3;
    public float restZ = 0.06f;
    public float drivenZ = -0.015f;

    int hits;
    float lastHit;
    AudioSource src;
    static AudioClip hitClip;

    public bool Driven => hits >= hitsNeeded;

    void Awake()
    {
        src = gameObject.AddComponent<AudioSource>();
        src.spatialBlend = 1f;
        if (!hitClip) hitClip = ProcAudio.Hit();
        Apply();
    }

    public void Refresh(bool show)
    {
        bool vis = show || hits > 0;
        if (visual.activeSelf != vis) visual.SetActive(vis);
        col.enabled = show && !Driven;
    }

    void Apply()
    {
        var p = visual.transform.localPosition;
        p.z = Mathf.Lerp(restZ, drivenZ, hits / (float)hitsNeeded);
        visual.transform.localPosition = p;
    }

    void OnTriggerEnter(Collider other)
    {
        if (Driven || Time.time - lastHit < 0.25f) return;
        var head = other.GetComponent<HammerHead>();
        if (!head || head.Speed < 0.9f) return;
        if (!plank.IsHeldByHand) return;   // il faut tenir la planche avec l'autre main
        hits++;
        lastHit = Time.time;
        src.PlayOneShot(hitClip);
        Apply();
    }
}
