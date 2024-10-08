using System;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;
using Zombies.Runtime.GameMeta;
using Zombies.Runtime.Health;

namespace Zombies.Runtime.Player
{
    public class PlayerPoints : NetworkBehaviour
    {
        public static int nonLethalHit = 10;
        public static int lethalHit = 60;
        public static int lethalHeadshot = 100;
        
        public int startingPoints = 500;
        public readonly SyncVar<int> currentPoints = new SyncVar<int>();
        [HideInInspector]
        public float displayedPoints;
        public float pointsDisplaySpeedConstant = 1f;
        public float pointsDisplaySpeedLinear = 1f;
        
        public float feedLifetime;
        public float feedFadeoutTime;
        public int maxLines;
        public bool reverse;
        public TMP_Text pointFeedElement;
        public TMP_Text pointsValue;
        
        private List<PointAward> pointFeed = new List<PointAward>();

        public override void OnStartServer()
        {
            currentPoints.Value = startingPoints;
            displayedPoints = startingPoints;
        }

        private void OnEnable()
        {
            HealthController.OnTakeDamage += TakeDamageEvent;
        }

        private void OnDisable()
        {
            HealthController.OnTakeDamage -= TakeDamageEvent;
        }

        private void Update()
        {
            var str = "";
            pointFeed.RemoveAll(e => Time.time - e.constructionTime > feedLifetime);
            var lines = maxLines > 0 ? Mathf.Min(pointFeed.Count, maxLines) : pointFeed.Count;
            for (var i0 = 0; i0 < lines; i0++)
            {
                var i1 = reverse ? lines - i0 - 1 : i0;
                var award = pointFeed[i1];
                var age = Time.time - award.constructionTime;
                var size = (i1 == 0 ? 1.5f : 1f) + Mathf.Pow(Mathf.InverseLerp(0.2f, 0f, age), 2f);
                var alpha = Mathf.RoundToInt(255f * Mathf.InverseLerp(feedLifetime, feedLifetime - feedFadeoutTime, age));
                
                str += $"<size={size * 100f:0}%><alpha=#{alpha:X2}>{award.ToString()}</size>\n";
            }

            pointFeedElement.text = str;

            displayedPoints = Mathf.MoveTowards(displayedPoints, currentPoints.Value, Time.deltaTime * (pointsDisplaySpeedConstant * 1000f + Mathf.Abs(displayedPoints - currentPoints.Value) * pointsDisplaySpeedLinear));
            pointsValue.text = displayedPoints.ToString("N0");
        }

        private void TakeDamageEvent(HealthController victim, HealthController.DamageReport report)
        {
            if (report.damage.invoker != gameObject) return;
            if (!Teams.AreHostile(gameObject, victim.gameObject)) return;

            if (report.wasLethal)
            {
                if (report.wasHeadshot)
                {
                    AwardPoints("Headshot Kill", lethalHeadshot);
                }
                else
                {
                    AwardPoints("Kill", lethalHit);
                }
            }
            else
            {
                AwardPoints("Hit", nonLethalHit);
            }
        }

        public void AwardPoints(string reason, int points)
        {
            if (!IsServerStarted) return;
            currentPoints.Value += points;
            AwardPointsRpc(reason, points);
        }

        [ObserversRpc(RunLocally = true)]
        private void AwardPointsRpc(string reason, int points)
        {
            pointFeed.Insert(0, new PointAward(reason, points));
        }


        public void DeductPoints(int cost)
        {
            if (!IsServerStarted) return;
            currentPoints.Value -= cost;
        }

        public struct PointAward
        {
            public int points;
            public string reason;
            public float constructionTime;

            public PointAward(string reason, int points)
            {
                this.reason = reason;
                this.points = points;

                constructionTime = Time.time;
            }

            public override string ToString() => $"+{points} {reason}";
        }
    }
}