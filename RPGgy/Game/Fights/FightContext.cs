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
        public IWarriorUser TurnOfEntity { get; private set; } // Ready guys ? 3, 2, 1... GO ! get, set, then get, then set, that's perfect ! continue ! ; get, set, mmm sir sorry i've hit a semicolon
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
            var myAwesomeResult = TurnOfEntity.AttackEntity(this, TurnOfEnemy);  
            return myAwesomeResult;
            
        }

        public void FinishedAction()
        {
            TurnChange();
        }
        private void TurnChange() // how does it works !
        {
            TurnOfEntity = TurnOfEnemy; // 1 idk swap
        } // now, void is awaiting you, but you can't await void (in some cases). paradox = true = paradox;
        public void SomeoneDied(IWarriorUser who)
        {
            OnDone(new FightContextTerminatedEventArgs(who,Opponent));
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
        public IWarriorUser WhoDiedUser { get; private set; }
        public IWarriorUser WinUser { get; private set; }
        public FightContextTerminatedEventArgs(IWarriorUser whoUser,IWarriorUser woonerUser)
        {
            WhoDiedUser = whoUser;
            WinUser = woonerUser;
        }
    }
}
