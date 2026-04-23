using UnityEngine;

namespace NervesOfDarkness
{
    public class TelescopeVisibilityToggler : MonoBehaviour
    {
        public Peephole peephole;
        public GameObject toggleObject;

        public void Update()
        {
            toggleObject.SetActive(peephole._peeping);
        }
    }
}