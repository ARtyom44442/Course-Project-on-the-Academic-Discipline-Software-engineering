using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace AlgoQuest.Models
{
    [Serializable]
    [XmlRoot("UserProfile")]
    public class UserProfile
    {
        [XmlElement("PlayerName")]
        public string PlayerName { get; set; }

        [XmlElement("TotalScore")]
        public int TotalScore { get; set; }

        [XmlElement("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [XmlArray("LevelResults")]
        [XmlArrayItem("LevelResult")]
        public List<LevelResult> LevelResults { get; set; }

        [XmlIgnore]
        public int CompletedLevelsCount
        {
            get { return LevelResults != null ? LevelResults.Count(r => r.IsCompleted) : 0; }
        }

        public UserProfile()
        {
            PlayerName = string.Empty;
            CreatedAt = DateTime.Now;
            LevelResults = new List<LevelResult>();
        }

        public UserProfile(string playerName) : this()
        {
            PlayerName = string.IsNullOrWhiteSpace(playerName) ? "Игрок" : playerName.Trim();
        }

        public LevelResult GetLevelResult(int levelId)
        {
            if (LevelResults == null)
            {
                return null;
            }

            return LevelResults.FirstOrDefault(r => r.LevelId == levelId);
        }

        public void RecordLevelAttempt(int levelId, string levelTitle, int score, int timeSeconds, bool isCompleted)
        {
            if (LevelResults == null)
            {
                LevelResults = new List<LevelResult>();
            }

            LevelResult existing = GetLevelResult(levelId);
            if (existing == null)
            {
                existing = new LevelResult(levelId, levelTitle);
                LevelResults.Add(existing);
            }
            else if (!string.IsNullOrWhiteSpace(levelTitle))
            {
                existing.LevelTitle = levelTitle;
            }

            if (score > existing.BestScore)
            {
                existing.BestScore = score;
            }

            if (isCompleted)
            {
                int normalizedTime = Math.Max(1, timeSeconds);
                if (!existing.IsCompleted || existing.BestTimeSeconds <= 0 || normalizedTime < existing.BestTimeSeconds)
                {
                    existing.BestTimeSeconds = normalizedTime;
                }

                existing.IsCompleted = true;
            }

            RecalculateTotalScore();
        }

        public void RecalculateTotalScore()
        {
            TotalScore = LevelResults != null ? LevelResults.Sum(r => r.BestScore) : 0;
        }
    }
}
