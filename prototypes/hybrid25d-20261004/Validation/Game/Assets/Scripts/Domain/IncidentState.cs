using System;
using System.Collections.Generic;

namespace MysteryRevival
{
    public enum DoorOutcome { Ignored, Safe, Escalated, Fatal }
    public sealed class IncidentState
    {
        private readonly HashSet<string> completedActions = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> observations = new HashSet<string>(StringComparer.Ordinal);
        private readonly List<string> facts = new List<string>();
        private readonly IReadOnlyList<string> readOnlyFacts;
        private double lastEscalation;

        public IncidentState()
        {
            readOnlyFacts = facts.AsReadOnly();
            Notes = string.Empty;
        }

        public double Time { get; private set; }
        public int Threat { get; private set; }
        public bool Paused { get; set; }
        public bool Dead { get { return Threat >= 3; } }
        public bool Inside { get; set; }
        public IReadOnlyList<string> Facts { get { return readOnlyFacts; } }
        public string Notes { get; set; }

        public void Advance(double seconds)
        {
            if (seconds < 0 || double.IsNaN(seconds) || double.IsInfinity(seconds))
                throw new ArgumentOutOfRangeException(nameof(seconds));
            if (!Paused && !Dead) Time += seconds;
        }

        public DoorOutcome OpenDoor(string actionId, int trueFloor, int declaredFloor, bool completed)
        {
            if (string.IsNullOrWhiteSpace(actionId)) throw new ArgumentException("Action id is required.", nameof(actionId));
            if (trueFloor < 1 || trueFloor > 3) throw new ArgumentOutOfRangeException(nameof(trueFloor));
            if (declaredFloor < 1 || declaredFloor > 3) throw new ArgumentOutOfRangeException(nameof(declaredFloor));

            // A completed interaction is consumed even when it causes no escalation.
            if (!completed || !completedActions.Add(actionId)) return DoorOutcome.Ignored;
            if (Paused || Dead) return DoorOutcome.Ignored;
            if (!Inside || trueFloor == declaredFloor) return DoorOutcome.Safe;
            if (Threat > 0 && Time - lastEscalation < 20) return DoorOutcome.Safe;

            lastEscalation = Time;
            Threat++;
            return Dead ? DoorOutcome.Fatal : DoorOutcome.Escalated;
        }

        public bool Observe(string id, string text)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Observation id is required.", nameof(id));
            if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Observed text is required.", nameof(text));
            if (Dead || !observations.Add(id)) return false;
            facts.Add(text);
            return true;
        }
    }
}
