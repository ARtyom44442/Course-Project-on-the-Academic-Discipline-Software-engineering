using System;
using System.Xml.Serialization;

namespace AlgoQuest.Models
{
    [Serializable]
    public class LevelResult
    {
        [XmlElement("LevelId")]
        public int LevelId { get; set; }

        [XmlElement("LevelTitle")]
        public string LevelTitle { get; set; }

        [XmlElement("IsCompleted")]
        public bool IsCompleted { get; set; }

        [XmlElement("BestScore")]
        public int BestScore { get; set; }

        [XmlElement("BestTimeSeconds")]
        public int BestTimeSeconds { get; set; }

        public LevelResult()
        {
            LevelTitle = string.Empty;
        }

        public LevelResult(int levelId, string levelTitle)
        {
            LevelId = levelId;
            LevelTitle = levelTitle ?? string.Empty;
            IsCompleted = false;
            BestScore = 0;
            BestTimeSeconds = 0;
        }
    }
}
