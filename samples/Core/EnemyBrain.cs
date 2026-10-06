using System;

namespace PortfolioSamples
{
    public enum EnemyState { Idle, Chase, Attack }

    public struct EnemyDecision
    {
        public EnemyState State { get; private set; }
        public bool ShouldAttack { get; private set; }
        public EnemyDecision(EnemyState state, bool shouldAttack)
        {
            State = state;
            ShouldAttack = shouldAttack;
        }
    }

    /// <summary>Decision logic only: movement, perception and damage belong to adapters.</summary>
    public sealed class EnemyBrain
    {
        private readonly double detectionRange;
        private readonly double forgetRange;
        private readonly double attackRange;
        private readonly double attackExitRange;
        private readonly double cooldown;
        private double remainingCooldown;

        public EnemyState State { get; private set; }

        public EnemyBrain(double detectionRange = 12, double forgetRange = 16,
            double attackRange = 2, double attackExitRange = 3, double cooldown = 1)
        {
            if (!Finite(detectionRange) || !Finite(forgetRange) || !Finite(attackRange)
                || !Finite(attackExitRange) || !Finite(cooldown)
                || attackRange <= 0 || attackExitRange <= attackRange
                || detectionRange <= attackExitRange || forgetRange <= detectionRange || cooldown <= 0)
                throw new ArgumentException("Require 0 < attack < attack exit < detection < forget, and cooldown > 0.");
            this.detectionRange = detectionRange;
            this.forgetRange = forgetRange;
            this.attackRange = attackRange;
            this.attackExitRange = attackExitRange;
            this.cooldown = cooldown;
        }

        public EnemyDecision Tick(double deltaSeconds, bool targetVisible, double distance)
        {
            if (!Finite(deltaSeconds) || deltaSeconds < 0)
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            if (targetVisible && (!Finite(distance) || distance < 0))
                throw new ArgumentOutOfRangeException(nameof(distance));

            remainingCooldown = Math.Max(0, remainingCooldown - deltaSeconds);
            if (!targetVisible || distance > (State == EnemyState.Idle ? detectionRange : forgetRange))
                State = EnemyState.Idle;
            else if (distance <= (State == EnemyState.Attack ? attackExitRange : attackRange))
                State = EnemyState.Attack;
            else
                State = EnemyState.Chase;

            bool attack = State == EnemyState.Attack && remainingCooldown <= 0;
            if (attack) remainingCooldown = cooldown;
            // At most one attack per tick: a long frame must not create a damage burst.
            return new EnemyDecision(State, attack);
        }

        public void Reset()
        {
            State = EnemyState.Idle;
            remainingCooldown = 0;
        }

        private static bool Finite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }
    }
}
