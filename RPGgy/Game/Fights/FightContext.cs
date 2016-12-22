using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord;
using RPGgy.Game.Core;
using RPGgy.Game.Player;
namespace RPGgy.Game.Fights
{
    public class TurnChangedEventArgs : EventArgs
    {
        public TurnChangedEventArgs(IWarriorUser current)
        {
            CurrentTurnUser = current;
        }
        public IWarriorUser CurrentTurnUser { get; private set; }
    }
    public class FightContextTerminatedEventArgs : EventArgs
    {
        public FightContextTerminatedEventArgs(IGameEntity whoUser, IGameEntity woonerUser)
        {
            WhoDiedUser = whoUser;
            WinUser = woonerUser;
        }

        public IGameEntity WhoDiedUser { get; private set; }
        public IGameEntity WinUser { get; private set; }
    }
    public sealed class FightContext
    {
        private readonly Tuple<IUser, IUser> _actualTuple;
        private Stopwatch TimeTook { get; } = new Stopwatch();
        public FightContext(IWarriorUser attackerParameter, IWarriorUser opponentParameter)
        {
            Attacker = attackerParameter;
            Opponent = opponentParameter;
            Attacker.AttachedFightContext = Opponent.AttachedFightContext = this;
            TurnOfEntity = Attacker;
            ActualContexts.Add(_actualTuple = new Tuple<IUser, IUser>(Attacker.AttachedUser, Opponent.AttachedUser),
                               this);
            TimeTook.Start();
        }

        public event EventHandler<TurnChangedEventArgs> OnTurnChanged;

        private IWarriorUser Attacker { get; }
        private IWarriorUser Opponent { get; }
        private bool _isSomeoneDead = false;
        public static Dictionary<Tuple<IUser, IUser>, FightContext> ActualContexts { get; } =
            new Dictionary<Tuple<IUser, IUser>, FightContext>();

        public IWarriorUser TurnOfEntity { get; private set; }
        // Ready guys ? 3, 2, 1... GO ! get, set, then get, then set, that's perfect ! continue ! ; get, set, mmm sir sorry i've hit a semicolon
        public IUser TurnOfUser => TurnOfEntity.AttachedUser;
        public IWarriorUser TurnOfEnemy => TurnOfEntity == Attacker ? Opponent : Attacker;
        public IUser TurnOfEnemyUser => TurnOfEnemy.AttachedUser;
/*
        private static Random Randomiser { get; } = new Random();
*/
        private bool _isAttacking;
        public void Attack(Action<AttackContext> act)
        {
            if (_isSomeoneDead) return;
            _isAttacking = true;
            Tuple<int, bool> myAwesomeResult = TurnOfEntity.AttackEntity(this, TurnOfEnemy,TimeTook.Elapsed);
            act(new AttackContext(myAwesomeResult.Item1, myAwesomeResult.Item2));
            TurnChange();
            _isAttacking = false;
            
        }

        private void TurnChange() // how does it works !
        {            
            TurnOfEntity = TurnOfEnemy;
            TimeTook.Restart();
            if (_isSomeoneDead) return;
            OnOnTurnChanged(new TurnChangedEventArgs(TurnOfEntity));
        } // now, void is awaiting you, but you can't await void (in some cases). paradox = true = paradox;

        public async Task SomeoneDied(IWarriorUser who)
        {
            _isSomeoneDead = true;
            OnDone(new FightContextTerminatedEventArgs(who, who == TurnOfEntity ? TurnOfEnemy : TurnOfEntity));
            ActualContexts.Remove(_actualTuple);
            while (_isAttacking)
                await Task.Delay(100);
            Attacker.AttachedFightContext = null;
            Opponent.AttachedFightContext = null;
        }

        public event EventHandler<FightContextTerminatedEventArgs> Done;

        private void OnDone(FightContextTerminatedEventArgs e)
        {
            Done?.Invoke(this, e);
        }

        public class AttackContext : object
        {
            public AttackContext(int value, bool critical)
            {
                AttackValue = value;
                IsCritical = critical;
            }

            public int AttackValue { get; private set; }
            public bool IsCritical { get; private set; }
        }


        private void OnOnTurnChanged(TurnChangedEventArgs e)
        {
            OnTurnChanged?.Invoke(this, e);
        }
    }
}

