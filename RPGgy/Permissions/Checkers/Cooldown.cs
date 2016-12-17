using System.Diagnostics;

namespace RPGgy.Permissions.Checkers
{
    /// <summary>
    ///     A cooldown
    /// </summary>
    /// <seealso cref="Stopwatch" />
    public sealed class Cooldown
    {
        private bool _c;

        /// <summary>
        ///     Creates a new instance of Cooldown
        /// </summary>
        /// <param name="seconds">The seconds of the cooldown</param>
        /// <param name="isAleradyCompleted">ye m8 will the first time complete m8</param>
        public Cooldown(int seconds, bool isAleradyCompleted = true)
        {
            CooldownSeconds = seconds;
            St.Start();
            _c = isAleradyCompleted;
        }

        /// <summary>
        ///     The private stopwatch of this class, the main compenent
        /// </summary>
        private Stopwatch St { get; } = new Stopwatch();

        /// <summary>
        ///     The cooldown seconds that has been set to this instance.
        /// </summary>
        public int CooldownSeconds { get; set; }

        /// <summary>
        ///     Returns true if the cooldown is finished
        /// </summary>
        public bool IsFinished
        {
            get
            {
                if (!_c)
                    return St.Elapsed.Seconds > CooldownSeconds;
                _c = false;
                return true;
            }
        }

        /// <summary>
        ///     Returns the seconds left for the cooldown to be reached, if it is alerady, returns null
        /// </summary>
        public int? SecondsLeft
        {
            get
            {
                if (!IsFinished)
                    return CooldownSeconds - St.Elapsed.Seconds;
                return null;
            }
        }

        public static Cooldown operator +(Cooldown a, Cooldown b)
        {
            return new Cooldown(a.CooldownSeconds + b.CooldownSeconds);
        }

        /// <summary>
        ///     Restarts the cooldown.
        /// </summary>
        public void Restart()
        {
            St.Restart();
        }

        /// <summary>
        ///     Resumes the cooldown into a string
        /// </summary>
        /// <returns>A string that resumes the cooldowns</returns>
        public override string ToString()
        {
            return $"Cooldown seconds : {CooldownSeconds}";
        }
    }
}