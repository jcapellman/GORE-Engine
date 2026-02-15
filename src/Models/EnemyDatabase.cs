using System.Collections.Generic;

namespace GORE.Models
{
    public class EnemyData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public int MaxHP { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Magic { get; set; }
        public int Speed { get; set; }
        public int ExpReward { get; set; }
        public int GoldReward { get; set; }
        public string Texture { get; set; }
        public List<int> EncounterZones { get; set; } = new();
    }

    public class EnemyDatabase
    {
        private static List<EnemyData> _enemies;

        public static void Load(List<EnemyData> enemies)
        {
            _enemies = enemies;
        }

        public static List<EnemyData> GetEnemiesForZone(int zone)
        {
            if (_enemies == null) return new List<EnemyData>();
            
            return _enemies.FindAll(e => e.EncounterZones.Contains(zone));
        }

        public static EnemyData GetEnemyById(int id)
        {
            return _enemies?.Find(e => e.Id == id);
        }

        public static Enemy CreateEnemyInstance(EnemyData data)
        {
            return new Enemy
            {
                Name = data.Name,
                Level = data.Level,
                MaxHP = data.MaxHP,
                CurrentHP = data.MaxHP,
                Attack = data.Attack,
                Defense = data.Defense,
                Magic = data.Magic,
                Speed = data.Speed,
                ExpReward = data.ExpReward,
                GoldReward = data.GoldReward,
                Texture = data.Texture
            };
        }
    }
}
