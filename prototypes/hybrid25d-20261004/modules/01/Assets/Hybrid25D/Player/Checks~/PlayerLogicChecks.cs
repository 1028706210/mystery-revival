using System;
using MysteryRevival.Hybrid25D;
using UnityEngine;

internal static class PlayerLogicChecks
{
    private static int passed;
    private static int failed;

    private static int Main()
    {
        // Removing input clamping, flattening, or basis normalization must fail these cases.
        Vector3 forward = new Vector3(1, -0.990243f, 1);
        Vector3 right = new Vector3(1, 0, -1);
        Check("W follows screen-up ground direction", new Vector2(0, 1), forward, right, 3,
            new Vector3(2.12132034f, 0, 2.12132034f));
        Check("D follows screen-right ground direction", new Vector2(1, 0), forward, right, 3,
            new Vector3(2.12132034f, 0, -2.12132034f));
        Check("S reverses W", new Vector2(0, -1), forward, right, 3,
            new Vector3(-2.12132034f, 0, -2.12132034f));
        Check("A reverses D", new Vector2(-1, 0), forward, right, 3,
            new Vector3(-2.12132034f, 0, 2.12132034f));
        Check("WD is not faster than W", new Vector2(1, 1), forward, right, 3,
            new Vector3(3, 0, 0));
        Check("WA diagonal", new Vector2(-1, 1), forward, right, 3,
            new Vector3(0, 0, 3));
        Check("SD diagonal", new Vector2(1, -1), forward, right, 3,
            new Vector3(0, 0, -3));
        Check("SA diagonal", new Vector2(-1, -1), forward, right, 3,
            new Vector3(-3, 0, 0));
        Check("small analog input preserves magnitude", new Vector2(0, 0.25f), forward, right, 3,
            new Vector3(0.530330086f, 0, 0.530330086f));
        Check("zero input stops", Vector2.zero, forward, right, 3, Vector3.zero);
        Check("overrange input is clamped", new Vector2(0, 5), forward, right, 3,
            new Vector3(2.12132034f, 0, 2.12132034f));
        Check("camera tilt does not reduce speed", new Vector2(0, 1), new Vector3(0, -10, 1),
            Vector3.right, 3, new Vector3(0, 0, 3));
        Check("vertical camera uses camera-right fallback", new Vector2(0, 1), Vector3.down,
            Vector3.right, 3, new Vector3(0, 0, 3));
        Check("invalid input cannot poison controller", new Vector2(float.NaN, 1), forward, right, 3,
            Vector3.zero);
        Check("negative speed stops", new Vector2(0, 1), forward, right, -3, Vector3.zero);
        Check("infinite speed stops", new Vector2(0, 1), forward, right, float.PositiveInfinity,
            Vector3.zero);
        Console.WriteLine("PLAYER_LOGIC passed=" + passed + " failed=" + failed);
        return failed == 0 ? 0 : 1;
    }

    private static void Check(string name, Vector2 input, Vector3 forward, Vector3 right, float speed,
        Vector3 expected)
    {
        Vector3 actual = HybridPlayerMovement.GroundVelocity(input, forward, right, speed);
        if (float.IsNaN(actual.x) || float.IsNaN(actual.y) || float.IsNaN(actual.z) ||
            (actual - expected).sqrMagnitude > 0.000001f)
        {
            ++failed;
            Console.WriteLine("FAIL " + name + " actual=" + actual + " expected=" + expected);
        }
        else
        {
            ++passed;
            Console.WriteLine("PASS " + name);
        }
    }
}
