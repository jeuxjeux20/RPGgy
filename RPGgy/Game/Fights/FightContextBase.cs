using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using RPGgy.Game.Player;

namespace RPGgy.Game.Fights
{
    public abstract class FightContextBase
    {
        private Tuple<IUser, IUser> _actualTuple;
        private bool _isSomeoneDead = false;
        private bool _isAttacking = false;

        public FightContextBase(IWarriorUser attackerParameter, IWarriorUser opponentParameter)
        {
            Attacker = attackerParameter;
            Opponent = opponentParameter;
            Attacker.AttachedFightContext = Opponent.AttachedFightContext = this;
            TurnOfEntity = Attacker;
            ActualContexts.Add(_actualTuple = new Tuple<IUser, IUser>(Attacker.AttachedUser, Opponent.AttachedUser),
                               this);
        }

        public virtual event EventHandler<TurnChangedEventArgs> OnTurnChanged;
        private IWarriorUser Attacker { get; }
        private IWarriorUser Opponent { get; }

        public static Dictionary<Tuple<IUser, IUser>, FightContext> ActualContexts { get; } =
            new Dictionary<Tuple<IUser, IUser>, FightContext>();

        public IWarriorUser TurnOfEntity { get; private set; }
        public IWarriorUser TurnOfEnemy => TurnOfEntity == Attacker ? Opponent : Attacker;

        public void Attack(Action<AttackContext> act)
        {
            if (_isSomeoneDead) return;
            _isAttacking = true;
            Tuple<int, bool> myAwesomeResult = TurnOfEntity.AttackEntity(this, TurnOfEnemy);
            act(new AttackContext(myAwesomeResult.Item1, myAwesomeResult.Item2));
            TurnChange();
            _isAttacking = false;
            
        }

        private void TurnChange() // how does it works !
        {            
            TurnOfEntity = TurnOfEnemy;
            if (_isSomeoneDead) return;
            OnOnTurnChanged(new TurnChangedEventArgs(TurnOfEntity));
        }

        public async Task SomeoneDied(IWarriorUser who)
        {
            _isSomeoneDead = true;
            OnDone(new FightContextTerminatedEventArgs(who, TurnOfEntity));
            ActualContexts.Remove(_actualTuple);
            while (_isAttacking)
                await Task.Delay(100);
            Attacker.AttachedFightContext = null;
            Opponent.AttachedFightContext = null;
        }

        public virtual event EventHandler<FightContextTerminatedEventArgs> Done;

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