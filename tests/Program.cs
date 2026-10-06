using System;
using System.Globalization;
using System.IO;
using PortfolioSamples;

internal static class Program
{
    private static int passed;
    private static int failed;

    private static int Main()
    {
        Test("stick ignores input without an owner", () => {
            var stick = new MobileStick(100);
            stick.Move(1, 80, 0);
            Near(0, stick.X);
            Check(!stick.IsHeld);
        });
        Test("stick keeps the first pointer and ignores secondary release", () => {
            var stick = new MobileStick(100, 0);
            Check(stick.Begin(1));
            Check(!stick.Begin(2));
            stick.Move(2, 100, 0);
            Near(0, stick.X);
            stick.Move(1, 50, 0);
            stick.End(2);
            Check(stick.IsHeld);
            Near(0.5, stick.X);
        });
        Test("dead zone is silent then remaps continuously", () => {
            var stick = new MobileStick(100, 0.2f);
            stick.Begin(1);
            stick.Move(1, 19, 0);
            Near(0, stick.X);
            stick.Move(1, 60, 0);
            Near(0.5, stick.X);
        });
        Test("diagonal magnitude is clamped without changing direction", () => {
            var stick = new MobileStick(100);
            stick.Begin(1);
            stick.Move(1, 200, -200);
            Near(1, Math.Sqrt(stick.X * stick.X + stick.Y * stick.Y));
            Near(stick.X, -stick.Y);
        });
        Test("release and cancellation clear stale movement", () => {
            var stick = new MobileStick(100);
            stick.Begin(-1);
            stick.Move(-1, 100, 0);
            stick.End(-1);
            Check(!stick.IsHeld);
            Near(0, stick.X);
            stick.Begin(3);
            stick.Move(3, 0, 100);
            stick.Cancel();
            Near(0, stick.Y);
            Check(stick.Begin(4));
        });
        Test("invalid input cannot leak NaN into movement", () => {
            var stick = new MobileStick(100);
            stick.Begin(1);
            stick.Move(1, float.NaN, 1);
            Near(0, stick.X);
            stick.Move(1, float.MaxValue, float.MaxValue);
            Near(1, Math.Sqrt(stick.X * stick.X + stick.Y * stick.Y));
        });
        Test("invalid stick configuration is rejected", () => {
            Throws<ArgumentOutOfRangeException>(() => new MobileStick(0));
            Throws<ArgumentOutOfRangeException>(() => new MobileStick(float.PositiveInfinity));
            Throws<ArgumentOutOfRangeException>(() => new MobileStick(100, 1));
            Throws<ArgumentOutOfRangeException>(() => new MobileStick(100, -0.1f));
        });
        Test("enemy detection and forgetting use hysteresis", () => {
            var brain = new EnemyBrain();
            Equal(EnemyState.Idle, brain.Tick(0, true, 13).State);
            Equal(EnemyState.Chase, brain.Tick(0, true, 12).State);
            Equal(EnemyState.Chase, brain.Tick(0, true, 15).State);
            Equal(EnemyState.Idle, brain.Tick(0, true, 17).State);
        });
        Test("attack state has a separate exit boundary", () => {
            var brain = new EnemyBrain();
            Equal(EnemyState.Chase, brain.Tick(0, true, 2.5).State);
            Check(brain.Tick(0, true, 2).ShouldAttack);
            Equal(EnemyState.Attack, brain.Tick(0, true, 2.5).State);
            Equal(EnemyState.Chase, brain.Tick(0, true, 3.1).State);
        });
        Test("cooldown remains active across loss of target", () => {
            var brain = new EnemyBrain();
            Check(brain.Tick(0, true, 1).ShouldAttack);
            Check(!brain.Tick(0.25, true, 1).ShouldAttack);
            Equal(EnemyState.Idle, brain.Tick(0.25, false, double.NaN).State);
            Check(!brain.Tick(0.25, true, 1).ShouldAttack);
            Check(brain.Tick(0.25, true, 1).ShouldAttack);
        });
        Test("long frame emits one attack and schedules the next cooldown", () => {
            var brain = new EnemyBrain();
            Check(brain.Tick(10, true, 1).ShouldAttack);
            Check(!brain.Tick(0, true, 1).ShouldAttack);
        });
        Test("pool reset clears enemy state and cooldown", () => {
            var brain = new EnemyBrain();
            brain.Tick(0, true, 1);
            brain.Reset();
            Equal(EnemyState.Idle, brain.State);
            Check(brain.Tick(0, true, 1).ShouldAttack);
        });
        Test("enemy validates time, distances and configuration", () => {
            var brain = new EnemyBrain();
            Throws<ArgumentOutOfRangeException>(() => brain.Tick(-1, true, 1));
            Throws<ArgumentOutOfRangeException>(() => brain.Tick(double.NaN, true, 1));
            Throws<ArgumentOutOfRangeException>(() => brain.Tick(1, true, double.PositiveInfinity));
            Throws<ArgumentException>(() => new EnemyBrain(attackRange: 4, attackExitRange: 3));
        });
        Test("progress round-trip preserves boundary values", () => {
            Progress decoded;
            Check(ProgressCodec.TryDecode(ProgressCodec.Encode(new Progress(10000, 1000000000)), out decoded));
            Equal(10000, decoded.UnlockedLevel);
            Equal(1000000000, decoded.Coins);
        });
        Test("save encoding is culture independent", () => {
            var previous = CultureInfo.CurrentCulture;
            try {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ar-EG");
                string text = ProgressCodec.Encode(new Progress(12, 345));
                Check(text.StartsWith("1|12|345|", StringComparison.Ordinal));
                Progress decoded;
                Check(ProgressCodec.TryDecode(text, out decoded));
            } finally { CultureInfo.CurrentCulture = previous; }
        });
        Test("corrupt, oversized and unknown-version saves are rejected", () => {
            string valid = ProgressCodec.Encode(new Progress(3, 100));
            foreach (string text in new[] { null, "", "1|", valid.Replace("|100|", "|101|"),
                "2" + valid.Substring(1), new string('x', 300), "1|2147483648|1|bad", "1|0|-1|bad" }) {
                Progress decoded;
                Check(!ProgressCodec.TryDecode(text, out decoded));
                Check(decoded == null);
            }
        });
        Test("invalid progress cannot be constructed", () => {
            Throws<ArgumentOutOfRangeException>(() => new Progress(0, 0));
            Throws<ArgumentOutOfRangeException>(() => new Progress(1, -1));
            Throws<ArgumentNullException>(() => ProgressCodec.Encode(null));
        });
        Test("missing save returns explicit default status", () => WithFiles(directory => {
            var result = new FileProgressStore(Path.Combine(directory, "not-created")).Load();
            Equal(SaveOrigin.Default, result.Origin);
            Equal(1, result.Progress.UnlockedLevel);
        }));
        Test("file save and replacement preserve latest progress", () => WithFiles(directory => {
            var store = new FileProgressStore(directory);
            store.Save(new Progress(2, 30));
            store.Save(new Progress(3, 60));
            Equal(SaveOrigin.Primary, store.Load().Origin);
            Equal(60, store.Load().Progress.Coins);
            Equal(0, Directory.GetFiles(directory, "*.tmp").Length);
        }));
        Test("corrupt primary falls back to last known-good backup", () => WithFiles(directory => {
            var store = new FileProgressStore(directory);
            store.Save(new Progress(2, 30));
            store.Save(new Progress(3, 60));
            File.WriteAllText(Path.Combine(directory, "progress.save"), "interrupted/corrupt");
            Equal(SaveOrigin.Backup, store.Load().Origin);
            Equal(30, store.Load().Progress.Coins);
        }));
        Test("saving after corruption retains the good backup", () => WithFiles(directory => {
            var store = new FileProgressStore(directory);
            string primary = Path.Combine(directory, "progress.save");
            store.Save(new Progress(2, 30));
            store.Save(new Progress(3, 60));
            File.WriteAllText(primary, "bad");
            store.Save(new Progress(4, 90));
            Equal(90, store.Load().Progress.Coins);
            File.WriteAllText(primary, "bad again");
            Equal(30, store.Load().Progress.Coins);
        }));
        Test("oversized primary and corrupt backup return default", () => WithFiles(directory => {
            File.WriteAllText(Path.Combine(directory, "progress.save"), new string('x', 1024));
            File.WriteAllText(Path.Combine(directory, "progress.save.bak"), "bad");
            Equal(SaveOrigin.Default, new FileProgressStore(directory).Load().Origin);
        }));
        Test("storage failures are visible to the caller", () => WithFiles(directory => {
            string file = Path.Combine(directory, "file-not-directory");
            File.WriteAllText(file, "owned test fixture");
            Throws<IOException>(() => new FileProgressStore(file).Save(new Progress()));
        }));

        Console.WriteLine("\n" + passed + " passed, " + failed + " failed.");
        return failed == 0 ? 0 : 1;
    }

    private static void Test(string name, Action body)
    {
        try { body(); passed++; Console.WriteLine("PASS " + name); }
        catch (Exception error) { failed++; Console.Error.WriteLine("FAIL " + name + ": " + error.Message); }
    }

    private static void Check(bool condition) { if (!condition) throw new Exception("Expected true."); }
    private static void Equal<T>(T expected, T actual)
    {
        if (!object.Equals(expected, actual)) throw new Exception("Expected " + expected + ", got " + actual);
    }
    private static void Near(double expected, double actual)
    {
        if (double.IsNaN(actual) || Math.Abs(expected - actual) > 0.00001)
            throw new Exception("Expected approximately " + expected + ", got " + actual);
    }
    private static void Throws<T>(Action action) where T : Exception
    {
        try { action(); } catch (T) { return; }
        throw new Exception("Expected " + typeof(T).Name);
    }
    private static void WithFiles(Action<string> body)
    {
        string directory = Path.Combine(Path.GetTempPath(), "portfolio-samples-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try { body(directory); }
        finally { Directory.Delete(directory, true); } // Only this test's unique, owned directory.
    }
}
