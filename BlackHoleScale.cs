using UnityEngine;

namespace NervesOfDarkness;
public class BlackHoleScale : MonoBehaviour
{
    private Transform blackHoleTransform;
    private SphereCollider[] blackHoleVolumes = new SphereCollider[2];
    private Material blackHoleMaterial;

    private float lerpElapsed = 0;
    private float warpDecrease = 0;
    private float i = 0;

    public void Start()
    {
        Transform blackHole = this.transform;
        blackHoleTransform = blackHole.Find("Sector/BlackHole/BlackHoleRenderer");
        blackHoleVolumes[0] = blackHole.Find("Sector/BlackHole/DestructionVolume").GetComponent<SphereCollider>();
        SpeedCheckTrigger speedCheck = blackHole.Find("Sector/Ring/Nebula/SpeedTriggerVolume").GetComponent<SpeedCheckTrigger>();
        speedCheck._volumeToDisable = blackHoleVolumes[0].gameObject;
        blackHoleVolumes[1] = speedCheck.transform.GetComponent<SphereCollider>();
        blackHoleMaterial = blackHoleTransform.GetComponent<MeshRenderer>().sharedMaterial;
    }

    public void Update()
    {
        lerpElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(lerpElapsed / Mathf.Max(0.0001f, 1320));
        float currentScale = Mathf.Lerp(1, 34.6153846154f, t);
        Vector3 currentScaleVector = new Vector3(currentScale, currentScale, currentScale);

        NervesOfDarkness.WriteLine("Current Scale: " + currentScale +
                                    "\nT Value: " + t, OWML.Common.MessageType.Success);

        if (blackHoleTransform != null)
        {
            blackHoleTransform.localScale = currentScaleVector * 2080;
            NervesOfDarkness.WriteLine("Black Hole Scale: " + blackHoleTransform.localScale, OWML.Common.MessageType.Success);
        }
        
        for (int i = 0; i <= 1; i++)
        {
            if (blackHoleVolumes[i] != null)
            {
                if (i == 0)
                {
                    blackHoleVolumes[i].radius = currentScale * 1040;
                } else if (i == 1)
                {
                    blackHoleVolumes[i].radius = currentScale * 1090;
                }
                NervesOfDarkness.WriteLine("Volume " + blackHoleVolumes[i].name + ": " + blackHoleVolumes[i].radius, OWML.Common.MessageType.Success);
            }
        }

        if (blackHoleMaterial != null)
        {
            blackHoleMaterial.SetFloat("_Radius", currentScale * 1040);
            warpDecrease = 2080 - i;
            NervesOfDarkness.WriteLine("Increment: " + i +
                                        "\nWarpDecrease: " + warpDecrease, OWML.Common.MessageType.Success);
            if (warpDecrease <= 1144)
            {
                blackHoleMaterial.SetFloat("_MaxDistortRadius", currentScale * 1144);
            } else
            {
                blackHoleMaterial.SetFloat("_MaxDistortRadius", currentScale * warpDecrease);
                if (TimeLoop.GetSecondsElapsed() % 2 == 0)
                {
                    i += 39;
                }
            }
            NervesOfDarkness.WriteLine("Radius: " + blackHoleMaterial.GetFloat("_Radius") + 
                                        "\nMax Distort Radius: " + blackHoleMaterial.GetFloat("_MaxDistortRadius"), OWML.Common.MessageType.Success);
        }
    }
}
