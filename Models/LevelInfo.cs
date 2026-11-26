using System;
using System.Xml.Serialization;

namespace AlgoQuest.Models
{
    [Serializable]
    [XmlRoot("LevelInfo")]
    public class LevelInfo
    {
        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("Title")]
        public string Title { get; set; }

        [XmlElement("Category")]
        public string Category { get; set; }

        [XmlElement("ShortGoal")]
        public string ShortGoal { get; set; }

        [XmlElement("TheoryText")]
        public string TheoryText { get; set; }

        [XmlElement("InitialData")]
        public string InitialData { get; set; }

        [XmlElement("TargetValue")]
        public string TargetValue { get; set; }

        [XmlElement("MaxScore")]
        public int MaxScore { get; set; }

        public LevelInfo()
        {
            Title = string.Empty;
            Category = string.Empty;
            ShortGoal = string.Empty;
            TheoryText = string.Empty;
            InitialData = string.Empty;
            TargetValue = string.Empty;
            MaxScore = 100;
        }
    }
}
