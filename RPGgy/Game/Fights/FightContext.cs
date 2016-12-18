using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using RPGgy.Game.Core;
using RPGgy.Game.Player;

namespace RPGgy.Game.Fights
{
    public sealed class FightContext
    {
        private IWarriorUser Attacker { get; }
        private IWarriorUser Opponent { get; }
        public static Dictionary<Tuple<IUser, IUser>, FightContext> ActualContexts { get; } = new Dictionary<Tuple<IUser, IUser>, FightContext>();
        private readonly Tuple<IUser, IUser> _actualTuple;
        public IWarriorUser TurnOfEntity { get; private set; }
        public IUser TurnOfUser => TurnOfEntity.AttachedUser;
        public IWarriorUser TurnOfEnemy => TurnOfEntity == Attacker ? Opponent : Attacker;
        public IUser TurnOfEnemyUser => TurnOfEnemy.AttachedUser;
        public FightContext(IWarriorUser attackerParameter, IWarriorUser opponentParameter)
        {
            Attacker = attackerParameter;
            Opponent = opponentParameter;
            Attacker.AttachedFightContext = Opponent.AttachedFightContext = this;
            TurnOfEntity = Attacker;
            ActualContexts.Add(_actualTuple = new Tuple<IUser, IUser>(Attacker.AttachedUser,Opponent.AttachedUser), this);
        }
        private static Random Randomiser { get; } = new Random();
        public Tuple<int,bool> Attack()
        {
            return TurnOfEntity.AttackEntity(this, TurnOfEnemy);
            TurnChange();
            
        }

        private void TurnChange()
        {
            TurnOfEntity = TurnOfEnemy;
        }
        public void SomeoneDied(IWarriorUser who)
        {
            OnDone(new FightContextTerminatedEventArgs(who));
            ActualContexts.Remove(_actualTuple);
            Attacker.AttachedFightContext = null;
            Opponent.AttachedFightContext = null;
        }

        public event EventHandler<FightContextTerminatedEventArgs> Done;

        private void OnDone(FightContextTerminatedEventArgs e)
        {
            Done?.Invoke(this, e);
        }
    }

    public class FightContextTerminatedEventArgs
    {
        public IWarriorUser WhoDidUser { get; private set; }

        public FightContextTerminatedEventArgs(IWarriorUser whoUser)
        {
            WhoDidUser = whoUser;
        }
    }
}
