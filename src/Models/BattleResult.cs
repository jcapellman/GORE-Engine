using System.Collections.Generic;

namespace GORE.Models
{
    public class BattleResult
    {
        public bool Victory { get; set; }
        public int TotalExp { get; set; }
        public int TotalGold { get; set; }
        public List<string> DefeatedEnemies { get; set; } = new();
        public Dictionary<string, int> CharacterExpGained { get; set; } = new();
        public List<string> LevelUps { get; set; } = new();
    }
}
