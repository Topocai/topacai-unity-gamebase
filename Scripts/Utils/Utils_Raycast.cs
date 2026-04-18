using UnityEngine;
using System.Linq;

namespace Topacai.Utils.Raycasts
{
    public static class RaycastsUtils
    {
        /// <summary>
        /// transform the current RaycastHit array into an ordered list by distance
        /// </summary>
        /// <param name="hits"></param>
        /// <param name="referencePoint"></param>
        /// <returns></returns> <summary>
        /// 
        /// </summary>
        /// <param name="hits"></param>
        /// <param name="referencePoint"></param>
        /// <returns></returns>
        public static RaycastHit[] SortByDistance(RaycastHit[] hits, Vector3 referencePoint)
            => hits.OrderBy(h => (h.point - referencePoint).sqrMagnitude).ToArray();

        /// <summary>
        /// transform the current RaycastHit array into an ordered list by distance
        /// </summary>
        /// <param name="hits"></param>
        /// <param name="reference"></param>
        /// <returns></returns> <summary>
        /// 
        /// </summary>
        /// <param name="hits"></param>
        /// <param name="reference"></param>
        /// <returns></returns>
        public static RaycastHit[] SortByDistance(RaycastHit[] hits, Transform reference)
            => SortByDistance(hits, reference.position);
    }
}
