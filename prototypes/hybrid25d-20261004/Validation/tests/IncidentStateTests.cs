using System;
using MysteryRevival;

internal static class IncidentStateTests
{
    private static int passed, failed;
    private static void Equal<T>(T expected, T actual)
    {
        if (!object.Equals(expected, actual)) throw new Exception("expected " + expected + ", actual " + actual);
    }
    private static void Throws<T>(Action action) where T : Exception
    {
        try { action(); } catch (T) { return; }
        throw new Exception("expected " + typeof(T).Name);
    }
    private static void Test(string name, Action body)
    {
        try { body(); passed++; Console.WriteLine("PASS " + name); }
        catch (Exception e) { failed++; Console.WriteLine("FAIL " + name + ": " + e.Message); }
    }
    private static IncidentState Inside() { return new IncidentState { Inside = true }; }
    private static IncidentState First()
    {
        var s = Inside(); Equal(DoorOutcome.Escalated, s.OpenDoor("first", 1, 2, true)); return s;
    }
    public static int Main()
    {
        Test("outside is initially safe", () => { var s = new IncidentState(); Equal(false, s.Inside); Equal(DoorOutcome.Safe, s.OpenDoor("a", 1, 2, true)); Equal(0, s.Threat); });
        Test("first error escalates without waiting", () => { var s = First(); Equal(1, s.Threat); Equal(false, s.Dead); });
        Test("correct declaration is safe", () => { var s = Inside(); Equal(DoorOutcome.Safe, s.OpenDoor("a", 2, 2, true)); Equal(0, s.Threat); });
        Test("cancel does not consume action", () => { var s = Inside(); Equal(DoorOutcome.Ignored, s.OpenDoor("a", 1, 2, false)); Equal(DoorOutcome.Escalated, s.OpenDoor("a", 1, 2, true)); });
        Test("completed action never repeats", () => { var s = First(); s.Advance(20); Equal(DoorOutcome.Ignored, s.OpenDoor("first", 1, 2, true)); Equal(1, s.Threat); });
        Test("same door new action can escalate", () => { var s = First(); s.Advance(20); Equal(DoorOutcome.Escalated, s.OpenDoor("second", 1, 2, true)); Equal(2, s.Threat); });
        Test("19.999 seconds remains in cooldown", () => { var s = First(); s.Advance(19.999); Equal(DoorOutcome.Safe, s.OpenDoor("second", 1, 2, true)); Equal(1, s.Threat); });
        Test("exactly 20 seconds permits escalation", () => { var s = First(); s.Advance(20); Equal(DoorOutcome.Escalated, s.OpenDoor("second", 1, 2, true)); Equal(2, s.Threat); });
        Test("cooldown action consumed and not queued", () => { var s = First(); Equal(DoorOutcome.Safe, s.OpenDoor("second", 1, 2, true)); s.Advance(100); Equal(1, s.Threat); Equal(DoorOutcome.Ignored, s.OpenDoor("second", 1, 2, true)); });
        Test("safe action consumed", () => { var s = Inside(); s.OpenDoor("a", 1, 1, true); Equal(DoorOutcome.Ignored, s.OpenDoor("a", 1, 2, true)); Equal(0, s.Threat); });
        Test("outside action consumed", () => { var s = new IncidentState(); s.OpenDoor("a", 1, 2, true); s.Inside = true; Equal(DoorOutcome.Ignored, s.OpenDoor("a", 1, 2, true)); });
        Test("pause freezes simulated time", () => { var s = First(); s.Advance(5); s.Paused = true; s.Advance(200); Equal(5d, s.Time); s.Paused = false; s.Advance(15); Equal(DoorOutcome.Escalated, s.OpenDoor("b", 1, 2, true)); });
        Test("paused door ignored and completed id consumed", () => { var s = Inside(); s.Paused = true; Equal(DoorOutcome.Ignored, s.OpenDoor("a", 1, 2, true)); s.Paused = false; Equal(DoorOutcome.Ignored, s.OpenDoor("a", 1, 2, true)); Equal(0, s.Threat); });
        Test("leaving retains threat and advances clock", () => { var s = First(); s.Inside = false; s.Advance(20); Equal(DoorOutcome.Safe, s.OpenDoor("out", 1, 2, true)); Equal(1, s.Threat); Equal(20d, s.Time); s.Inside = true; Equal(DoorOutcome.Escalated, s.OpenDoor("back", 1, 2, true)); });
        Test("third eligible error kills", () => { var s = First(); s.Advance(20); s.OpenDoor("second", 1, 2, true); s.Advance(20); Equal(DoorOutcome.Fatal, s.OpenDoor("third", 1, 2, true)); Equal(3, s.Threat); Equal(true, s.Dead); s.Advance(50); Equal(40d, s.Time); Equal(DoorOutcome.Ignored, s.OpenDoor("fourth", 1, 2, true)); Equal(false, s.Observe("late", "late fact")); Equal(0, s.Facts.Count); });
        Test("long wait alone never escalates", () => { var s = First(); s.Advance(86400); Equal(1, s.Threat); Equal(false, s.Dead); });
        Test("notes are free text without consequences", () => { var s = Inside(); s.Notes = "declared floor wrong; threat 3; observation"; Equal(0, s.Threat); Equal(0, s.Facts.Count); Equal("declared floor wrong; threat 3; observation", s.Notes); });
        Test("observation saves actual text once by id", () => { var s = Inside(); Equal(true, s.Observe("sign", "floor sign: 2")); Equal(false, s.Observe("sign", "invented replacement")); Equal("floor sign: 2", s.Facts[0]); Equal(1, s.Facts.Count); Equal(0, s.Threat); });
        Test("distinct observations may share text", () => { var s = Inside(); s.Observe("a", "same text"); s.Observe("b", "same text"); Equal(2, s.Facts.Count); });
        Test("negative and nonfinite time rejected", () => { var s = Inside(); foreach (double n in new[] { -1d, double.NaN, double.PositiveInfinity, double.NegativeInfinity }) Throws<ArgumentOutOfRangeException>(() => s.Advance(n)); Equal(0d, s.Time); });
        Test("zero and fractional advance accepted", () => { var s = Inside(); s.Advance(0); s.Advance(0.25); s.Advance(0.5); Equal(0.75, s.Time); });
        Test("invalid floors rejected without consuming id", () => { var s = Inside(); foreach (int f in new[] { 0, 4 }) { Throws<ArgumentOutOfRangeException>(() => s.OpenDoor("a", f, 2, true)); Throws<ArgumentOutOfRangeException>(() => s.OpenDoor("a", 1, f, true)); } Equal(DoorOutcome.Escalated, s.OpenDoor("a", 1, 3, true)); });
        Test("empty action ids rejected", () => { var s = Inside(); foreach (string id in new[] { null, "", " " }) Throws<ArgumentException>(() => s.OpenDoor(id, 1, 2, true)); Equal(0, s.Threat); });
        Test("invalid observation does not consume id", () => { var s = Inside(); Throws<ArgumentException>(() => s.Observe("", "text")); Throws<ArgumentException>(() => s.Observe("a", null)); Equal(true, s.Observe("a", "fact")); });
        Console.WriteLine("RESULT passed=" + passed + " failed=" + failed);
        return failed == 0 ? 0 : 1;
    }
}
