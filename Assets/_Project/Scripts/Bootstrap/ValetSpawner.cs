using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Valet;
using UnityEngine;

namespace UncannyValet.Bootstrap
{
    /// <summary>Puts the valet at the lot's start point, gives him a placeholder body when he has no animator, and points the camera at him.</summary>
    public static class ValetSpawner
    {
        private static readonly Color PlaceholderColor = new Color(0.95f, 0.85f, 0.2f);

        public static void Spawn(GameObject valet, CameraFollow camera)
        {
            valet.transform.position = Services.Get<ILotLayout>().ValetStartPoint;
            if (valet.GetComponent<ValetAnimator>() == null) // the animator owns the sprites when present
            {
                var body = valet.GetComponentInChildren<SpriteRenderer>();
                body.sprite = PlaceholderSprite.Create();
                body.color = PlaceholderColor;
            }
            valet.SetActive(true);
            if (camera != null) camera.SetTarget(valet.transform);
        }
    }
}
