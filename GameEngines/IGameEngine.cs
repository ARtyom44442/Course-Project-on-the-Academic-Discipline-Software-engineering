using AlgoQuest.Models;

namespace AlgoQuest.GameEngines
{
    public interface IGameEngine
    {
        LevelInfo LevelInfo { get; }
        int MistakesCount { get; }
        int CurrentScore { get; }
        bool IsCompleted { get; }

        void Initialize(LevelInfo levelInfo);
        string GetHint();
    }
}
