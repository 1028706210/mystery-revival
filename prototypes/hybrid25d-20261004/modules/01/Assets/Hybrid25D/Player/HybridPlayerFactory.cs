using System;
using UnityEngine;

namespace MysteryRevival.Hybrid25D
{
    public static class HybridPlayerFactory
    {
        // P1 candidates, measured from the reused 48x96 source PNG (visible height 72, foot 24/8).
        public const float VisibleHeight = 1.7f;
        public const float CollisionRadius = 0.22f;

        public static GameObject Create(Transform parent, Camera view, Sprite sprite, Vector3 spawn)
        {
            if (view == null) throw new ArgumentNullException(nameof(view));
            if (sprite == null) throw new ArgumentNullException(nameof(sprite));
            if (sprite.rect.width != 48 || sprite.rect.height != 96)
                throw new ArgumentException("P1 requires the supplied 48x96 nightwalker Idle Sprite.", nameof(sprite));

            var player = new GameObject("Player");
            player.transform.SetParent(parent, false);
            player.transform.position = spawn;

            var controller = player.AddComponent<CharacterController>();
            controller.height = VisibleHeight;
            controller.radius = CollisionRadius;
            controller.center = new Vector3(0, VisibleHeight * 0.5f, 0);
            controller.skinWidth = 0.02f;
            controller.stepOffset = 0.25f;
            controller.slopeLimit = 45;
            controller.minMoveDistance = 0;

            var visual = new GameObject("Visual");
            visual.transform.SetParent(player.transform, false);
            visual.transform.localScale = Vector3.one * (VisibleHeight * sprite.pixelsPerUnit / 72f);
            visual.AddComponent<HybridPlayerVisual>().Initialize(view);

            var image = new GameObject("IdleSprite");
            image.transform.SetParent(visual.transform, false);
            // Cancel any importer pivot offset: the PNG's actual foot stays at the root, including when billboarding.
            image.transform.localPosition = new Vector3(sprite.pivot.x - 24, sprite.pivot.y - 8, 0) / sprite.pixelsPerUnit;
            image.AddComponent<SpriteRenderer>().sprite = sprite;

            player.AddComponent<HybridPlayerMotor>().Initialize(view);
            return player;
        }
    }
}
