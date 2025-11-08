using UnityEngine;

namespace Metaverse.Interactions.Darts
{
    public class DartHolder : MonoBehaviour
    {
        [SerializeField] private Dart[] darts;
        [SerializeField] private Transform[] dartsTr;

        public void Init()
        {
            for (var i = 0; i < darts.Length; i++)
            {
                var dart = darts[i];
                dart.transform.position = dartsTr[i].position;
                dart.transform.rotation = dartsTr[i].rotation;

                dart.Init();
            }
        }
    }
}