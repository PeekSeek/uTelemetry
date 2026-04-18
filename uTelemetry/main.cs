using Rocket.Core.Plugins;
using SDG.Unturned;
using System;
using UnityEngine;
using System.Reflection;
using Logger = Rocket.Core.Logging.Logger;
using System.Threading.Tasks;

namespace uTelemetry;

public class uTelemetry : RocketPlugin<Configuration>
{
    public static uTelemetry Instance { get; private set; }
    public UnityEngine.Color MessageColor { get; set; }

    protected override void Load()
    {

        UseableGun.onBulletSpawned += OnBulletSpawned;

        Logger.Log($"miauw has been loaded!", ConsoleColor.Yellow);

        Task.Run(async () =>
        {
            await DiscordWebhookExample.SendMessage("hello world");
        });

        
    }

    private Vector3 ClosestPointOnSegment(Vector3 a, Vector3 b, Vector3 point)
    {
        Vector3 ab = b - a;
        float t = Vector3.Dot(point - a, ab) / Vector3.Dot(ab, ab);
        t = Mathf.Clamp01(t);
        return a + ab * t;
    }

    public static float CalculateBulletDrop(float gravityMultiplier, float distance, float velocity)
    {
        var g = Physics.gravity.y;

        float time = distance / velocity;

        float drop = 0.5f * g * gravityMultiplier * time * time;

        return drop;
    }

    private Vector3 GetAimPoint(Vector3 origin, PlayerLook look, float maxDistance = 500f)
    {
        if (Physics.Raycast(origin, look.aim.forward, out RaycastHit hit, maxDistance))
            return hit.point;

        return origin + look.aim.forward * maxDistance;
    }

    private void OnBulletSpawned(UseableGun gun, BulletInfo bullet)
    {
        var player = gun.player;

        Vector3 origin = bullet.origin;

        // InputInfo input = player.input.getInput(false, ERaycastInfoUsage.Gun, bullet.origin);

        // Vector3 direction = player.look.aim.forward;//input.direction;
        // Vector3 direction = GetAimPoint(bullet.origin, player.look);//gun.transform.forward;

        // Vector3 direction = player.look.aim.forward.normalized;
        
        var equippedGunAsset = gun.equippedGunAsset;

        Transform barricade = GetFirstBarricade();
        Vector3 target = barricade.position;

        var dropOff = CalculateBulletDrop(
            equippedGunAsset.bulletGravityMultiplier,
            (bullet.origin - target).magnitude,
            equippedGunAsset.muzzleVelocity
        );

        Vector3 aimPoint = GetAimPoint(bullet.origin, player.look);
        Vector3 direction = (aimPoint - bullet.origin).normalized;

        Vector3 closestPoint = ClosestPointOnRay(origin, direction, target);

        Vector3 offset = target - closestPoint;

        Vector3 forward = direction;
        Vector3 right = Vector3.Cross(forward, Vector3.up).normalized;
        Vector3 up = Vector3.Cross(right, forward).normalized;
        

        float xOffset = Vector3.Dot(offset, right);
        float yOffset = Vector3.Dot(offset, up);

        Logger.Log($"{player.channel.owner.playerID.characterName} shot dummy -> X: {xOffset:F6}, Y: {yOffset:F6}");
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
        ushort effectId = 115; // replace with a real effect ID

        #pragma warning disable CS0618 // Type or member is obsolete
        EffectManager.sendEffect(
            effectId,
            (short)0,
            position
        );
        #pragma warning restore CS0618 // Type or member is obsolete
    }
}