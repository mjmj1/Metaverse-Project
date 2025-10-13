using DG.Tweening;
using UnityEngine;

namespace Metaverse.Interactions
{
    public class SlideView : MonoBehaviour
    {
        private DOTweenPath path;

        private void Awake()
        {
            path = GetComponent<DOTweenPath>();
        }
    }
}
