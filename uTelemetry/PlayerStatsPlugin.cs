using Rocket.Core.Plugins;
using SDG.Unturned;
using System;
using UnityEngine;

namespace uTelemetry
{
    public class uTelemetry : RocketPlugin<Configuration>
    {
        public static uTelemetry Instance { get; private set; }
        public UnityEngine.Color MessageColor { get; set; }

        protected override void Load()
        {

            UseableGun.onBulletSpawned += OnBulletSpawned;

            Rocket.Core.Logging.Logger.Log($"miauw has been loaded!", ConsoleColor.Yellow);
        }


        private void OnBulletSpawned(UseableGun gun, BulletInfo bullet)
        {
            var player = gun.player;

            Vector3 origin = player.look.aim.position;
            Vector3 direction = player.look.aim.forward.normalized;

            Transform barricade = GetFirstBarricade();

            Vector3 dummyPos = barricade.position;

            Vector3 closestPoint = ClosestPointOnRay(origin, direction, dummyPos);

            Vector3 offset = dummyPos - closestPoint;

            Vector3 forward = direction;
            Vector3 right = Vector3.Cross(forward, Vector3.up).normalized;
            Vector3 up = Vector3.Cross(right, forward).normalized;

            float xOffset = Vector3.Dot(offset, right);
            float yOffset = Vector3.Dot(offset, up);

            Rocket.Core.Logging.Logger.Log(
                $"{player.channel.owner.playerID.characterName} shot dummy -> X: {xOffset:F3}, Y: {yOffset:F3}"
            );

            DrawPoint(closestPoint);
        }

        private Vector3 ClosestPointOnRay(Vector3 origin, Vector3 dir, Vector3 point)
        {
            dir.Normalize();
            float t = Vector3.Dot(point - origin, dir);
            return origin + dir * t;
        }

        private Transform GetFirstBarricade()
        {
            foreach (var region in BarricadeManager.regions)
            {
                if (region == null) continue;

                foreach (var drop in region.drops)
                {
                    if (drop.interactable == null) continue;

                    return drop.model;
                }
            }

            return null;
        }

        private void DrawPoint(Vector3 position, float duration = 1.5f)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            marker.transform.position = position;
            marker.transform.localScale = Vector3.one * 0.2f;

            UnityEngine.Object.Destroy(marker.GetComponent<Collider>());

            var renderer = marker.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.red;
            }

            UnityEngine.Object.Destroy(marker, duration);
        }
    }
}