using System;
using System.Collections.Generic;
using System.Linq;
using GORE.Models;

namespace GORE.GameEngine
{
    public enum BattleState
    {
        Start,
        PlayerTurn,
        EnemyTurn,
        Victory,
        Defeat
    }

    public class BattleSystem
    {
        public List<Enemy> Enemies { get; private set; } = new();
        public List<Character> Party { get; private set; } = new();
        public BattleState State { get; private set; }
        public int CurrentCharacterIndex { get; private set; }
        public List<string> BattleLog { get; private set; } = new();

        private readonly Random random;

        public BattleSystem(Random random)
        {
            this.random = random;
        }

        public BattleSystem()
        {
            random = new Random();
        }

        public void StartBattle(List<Character> party, List<Enemy> enemies)
        {
            Party = new List<Character>(party);
            Enemies = new List<Enemy>(enemies);
            State = BattleState.Start;
            CurrentCharacterIndex = 0;
            BattleLog.Clear();

            BattleLog.Add($"Battle started against {enemies.Count} enemies!");
            State = BattleState.PlayerTurn;
        }

        public string ExecuteAttack(Character attacker, Enemy target)
        {
            if (target == null || !target.IsAlive)
                return "Invalid target!";

            int damage = attacker.CalculateDamage(target.Defense);
            int variance = random.Next(-2, 3);
            damage = Math.Max(1, damage + variance);

            target.CurrentHP = Math.Max(0, target.CurrentHP - damage);

            string message = $"{attacker.Name} attacks {target.Name} for {damage} damage!";
            if (!target.IsAlive)
                message += $"\n{target.Name} defeated!";

            BattleLog.Add(message);
            return message;
        }

        public List<string> ExecuteEnemyTurn()
        {
            List<string> messages = new();

            foreach (var enemy in Enemies.Where(e => e.IsAlive))
            {
                var aliveParty = Party.Where(c => c.IsAlive).ToList();
                if (aliveParty.Count == 0) break;

                var target = aliveParty[random.Next(aliveParty.Count)];

                int damage = enemy.CalculateDamage(target.Defense);
                int variance = random.Next(-2, 3);
                damage = Math.Max(1, damage + variance);

                target.CurrentHP = Math.Max(0, target.CurrentHP - damage);

                string message = $"{enemy.Name} attacks {target.Name} for {damage} damage!";
                if (!target.IsAlive)
                    message += $"\n{target.Name} has fallen!";

                messages.Add(message);
                BattleLog.Add(message);
            }

            return messages;
        }

        public bool IsBattleOver()
        {
            bool allEnemiesDead = Enemies.All(e => !e.IsAlive);
            bool allPartyDead = Party.All(c => !c.IsAlive);

            if (allEnemiesDead)
            {
                State = BattleState.Victory;
                return true;
            }

            if (allPartyDead)
            {
                State = BattleState.Defeat;
                return true;
            }

            return false;
        }

        public BattleResult GetBattleResult()
        {
            var result = new BattleResult
            {
                Victory = State == BattleState.Victory
            };

            if (result.Victory)
            {
                foreach (var enemy in Enemies)
                {
                    result.TotalExp += enemy.ExpReward;
                    result.TotalGold += enemy.GoldReward;
                    result.DefeatedEnemies.Add(enemy.Name);
                }

                int expPerCharacter = result.TotalExp / Party.Count;
                foreach (var character in Party.Where(c => c.IsAlive))
                {
                    int oldLevel = character.Level;
                    character.Experience += expPerCharacter;
                    result.CharacterExpGained[character.Name] = expPerCharacter;

                    int expForNextLevel = 100 * character.Level;
                    while (character.Experience >= expForNextLevel)
                    {
                        character.Experience -= expForNextLevel;
                        character.LevelUp();
                        result.LevelUps.Add($"{character.Name} reached level {character.Level}!");
                        expForNextLevel = 100 * character.Level;
                    }
                }
            }

            return result;
        }

        public void NextCharacterTurn()
        {
            CurrentCharacterIndex++;
            if (CurrentCharacterIndex >= Party.Count || !Party.Skip(CurrentCharacterIndex).Any(c => c.IsAlive))
            {
                State = BattleState.EnemyTurn;
            }
        }
    }
}
