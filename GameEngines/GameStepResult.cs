namespace AlgoQuest.GameEngines
{
    public class GameStepResult
    {
        public bool IsValidStep { get; set; }
        public bool IsLevelCompleted { get; set; }
        public string Message { get; set; }
        public int CurrentScore { get; set; }

        public GameStepResult()
        {
            Message = string.Empty;
        }

        public GameStepResult(bool isValidStep, bool isLevelCompleted, string message, int currentScore)
        {
            IsValidStep = isValidStep;
            IsLevelCompleted = isLevelCompleted;
            Message = message ?? string.Empty;
            CurrentScore = currentScore;
        }
    }
}
