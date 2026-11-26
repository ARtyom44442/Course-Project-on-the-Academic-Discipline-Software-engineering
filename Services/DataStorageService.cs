using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Serialization;
using AlgoQuest.Models;

namespace AlgoQuest.Services
{
    public class DataStorageService
    {
        private const string DataFolderName = "Data";
        private const string ProfilesFileName = "profiles.xml";
        private const string LevelsFileName = "levels.xml";

        private static readonly XmlSerializer ProfilesSerializer =
            new XmlSerializer(typeof(List<UserProfile>), new XmlRootAttribute("UserProfiles"));

        private static readonly XmlSerializer LevelsSerializer =
            new XmlSerializer(typeof(List<LevelInfo>), new XmlRootAttribute("Levels"));

        public string DataDirectoryPath { get; private set; }
        public string ProfilesFilePath { get; private set; }
        public string LevelsFilePath { get; private set; }

        public DataStorageService() : this(AppDomain.CurrentDomain.BaseDirectory)
        {
        }

        public DataStorageService(string baseDirectory)
        {
            if (string.IsNullOrWhiteSpace(baseDirectory))
            {
                baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            }

            DataDirectoryPath = Path.Combine(baseDirectory, DataFolderName);
            EnsureDataDirectoryExists();

            ProfilesFilePath = Path.Combine(DataDirectoryPath, ProfilesFileName);
            LevelsFilePath = Path.Combine(DataDirectoryPath, LevelsFileName);
        }

        private void EnsureDataDirectoryExists()
        {
            if (!Directory.Exists(DataDirectoryPath))
            {
                Directory.CreateDirectory(DataDirectoryPath);
            }
        }

        #region Работа с профилями пользователей

        public List<UserProfile> LoadProfiles()
        {
            EnsureDataDirectoryExists();

            if (!File.Exists(ProfilesFilePath))
            {
                return new List<UserProfile>();
            }

            try
            {
                using (var stream = new FileStream(ProfilesFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    if (stream.Length == 0)
                    {
                        return new List<UserProfile>();
                    }

                    var profiles = ProfilesSerializer.Deserialize(stream) as List<UserProfile>;
                    return profiles ?? new List<UserProfile>();
                }
            }
            catch (InvalidOperationException)
            {
                return new List<UserProfile>();
            }
        }

        public void SaveProfiles(List<UserProfile> profiles)
        {
            if (profiles == null)
            {
                throw new ArgumentNullException("profiles");
            }

            EnsureDataDirectoryExists();

            foreach (UserProfile profile in profiles)
            {
                profile.RecalculateTotalScore();
            }

            string tempPath = ProfilesFilePath + ".tmp";
            using (var writer = new StreamWriter(tempPath, false, Encoding.UTF8))
            {
                ProfilesSerializer.Serialize(writer, profiles);
            }

            if (File.Exists(ProfilesFilePath))
            {
                File.Delete(ProfilesFilePath);
            }

            File.Move(tempPath, ProfilesFilePath);
        }

        public UserProfile GetOrCreateProfile(string playerName)
        {
            string normalizedName = string.IsNullOrWhiteSpace(playerName) ? "Игрок" : playerName.Trim();
            List<UserProfile> profiles = LoadProfiles();

            UserProfile existing = profiles.FirstOrDefault(
                p => string.Equals(p.PlayerName, normalizedName, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                return existing;
            }

            var created = new UserProfile(normalizedName);
            profiles.Add(created);
            SaveProfiles(profiles);
            return created;
        }

        public void SaveOrUpdateProfile(UserProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException("profile");
            }

            profile.RecalculateTotalScore();
            List<UserProfile> profiles = LoadProfiles();

            int index = profiles.FindIndex(
                p => string.Equals(p.PlayerName, profile.PlayerName, StringComparison.OrdinalIgnoreCase));

            if (index >= 0)
            {
                profiles[index] = profile;
            }
            else
            {
                profiles.Add(profile);
            }

            SaveProfiles(profiles);
        }

        #endregion

        #region Работа с уровнями и SeedData

        public List<LevelInfo> LoadLevels()
        {
            EnsureDataDirectoryExists();

            if (!File.Exists(LevelsFilePath))
            {
                List<LevelInfo> defaultLevels = SeedData();
                SaveLevels(defaultLevels);
                return defaultLevels;
            }

            try
            {
                using (var stream = new FileStream(LevelsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var levels = LevelsSerializer.Deserialize(stream) as List<LevelInfo>;
                    if (levels == null || levels.Count == 0)
                    {
                        List<LevelInfo> defaultLevels = SeedData();
                        SaveLevels(defaultLevels);
                        return defaultLevels;
                    }

                    return levels.OrderBy(l => l.Id).ToList();
                }
            }
            catch (InvalidOperationException)
            {
                List<LevelInfo> defaultLevels = SeedData();
                SaveLevels(defaultLevels);
                return defaultLevels;
            }
        }

        public void SaveLevels(List<LevelInfo> levels)
        {
            if (levels == null)
            {
                throw new ArgumentNullException("levels");
            }

            EnsureDataDirectoryExists();

            using (var writer = new StreamWriter(LevelsFilePath, false, Encoding.UTF8))
            {
                LevelsSerializer.Serialize(writer, levels);
            }
        }

        public List<LevelInfo> SeedData()
        {
            return new List<LevelInfo>
            {
                new LevelInfo
                {
                    Id = 1,
                    Title = "Сортировка пузырьком (Bubble Sort)",
                    Category = LevelCategories.Sorting,
                    ShortGoal = "Упорядочите сундуки с золотом по возрастанию веса, меняя местами соседние пары.",
                    TheoryText =
                        "ПРИНЦИП РАБОТЫ:\n" +
                        "Алгоритм многократно проходит по массиву слева направо, сравнивая пары соседних элементов. " +
                        "Если левый элемент больше правого, они меняются местами. После каждого прохода самый тяжелый " +
                        "«пузырек» всплывает в конец неотсортированной части.\n\n" +
                        "ОЦЕНКА СЛОЖНОСТИ:\n" +
                        "• В худшем и среднем случаях: O(n²)\n" +
                        "• В лучшем случае (уже отсортированный массив с флагом обмена): O(n)\n" +
                        "• Дополнительная память: O(1) — сортировка выполняется на месте.\n\n" +
                        "ПРАВИЛА ПРОХОЖДЕНИЯ:\n" +
                        "Сравнивайте только соседние сундуки, начиная с начала ряда. Меняйте их местами, " +
                        "если вес левого сундука строго больше правого.",
                    InitialData = "42,15,8,23,4,16",
                    MaxScore = 100
                },
                new LevelInfo
                {
                    Id = 2,
                    Title = "Сортировка выбором (Selection Sort)",
                    Category = LevelCategories.Sorting,
                    ShortGoal = "На каждом шаге находите минимальный груз в оставшейся части и ставьте его на текущую позицию.",
                    TheoryText =
                        "ПРИНЦИП РАБОТЫ:\n" +
                        "Массив условно делится на отсортированную (слева) и неотсортированную (справа) части. " +
                        "На i-м шаге алгоритм ищет минимальный элемент в неотсортированной части и меняет его местами " +
                        "с элементом на позиции i.\n\n" +
                        "ОЦЕНКА СЛОЖНОСТИ:\n" +
                        "• Во всех случаях (лучшем, среднем, худшем): O(n²)\n" +
                        "• Количество обменов минимально среди квадратичных сортировок: не более O(n)\n" +
                        "• Дополнительная память: O(1).\n\n" +
                        "ПРАВИЛА ПРОХОЖДЕНИЯ:\n" +
                        "Выберите самый легкий груз среди еще не отсортированных и переместите его на первую свободную " +
                        "позицию слева.",
                    InitialData = "35,12,48,7,19,25",
                    MaxScore = 100
                },
                new LevelInfo
                {
                    Id = 3,
                    Title = "Сортировка вставками (Insertion Sort)",
                    Category = LevelCategories.Sorting,
                    ShortGoal = "Берите следующий слиток и вставляйте его на нужное место в уже отсортированную часть.",
                    TheoryText =
                        "ПРИНЦИП РАБОТЫ:\n" +
                        "Алгоритм поочередно берет элементы начиная со второго и «вставляет» каждый в уже " +
                        "упорядоченную левую часть массива, сдвигая более тяжелые элементы вправо.\n\n" +
                        "ОЦЕНКА СЛОЖНОСТИ:\n" +
                        "• В худшем и среднем случаях: O(n²)\n" +
                        "• В лучшем случае (почти отсортированный массив): O(n)\n" +
                        "• Дополнительная память: O(1), алгоритм устойчивый.\n\n" +
                        "ПРАВИЛА ПРОХОЖДЕНИЯ:\n" +
                        "Сравнивайте текущий слиток с элементами слева от него и смещайте его влево до тех пор, " +
                        "пока не будет найдено правильное место по возрастанию веса.",
                    InitialData = "29,10,14,37,13,5",
                    MaxScore = 100
                },
                new LevelInfo
                {
                    Id = 4,
                    Title = "Бинарный поиск (Binary Search)",
                    Category = LevelCategories.Search,
                    ShortGoal = "Найдите закрытый сундук весом 23 кг за минимальное число проверок.",
                    TheoryText =
                        "ПРИНЦИП РАБОТЫ:\n" +
                        "Бинарный поиск работает только на отсортированных данных. На каждом шаге проверяется средний " +
                        "элемент текущего диапазона [left; right]. Если он равен искомому — поиск завершен. " +
                        "Если искомое значение меньше среднего, отбрасывается правая половина, иначе — левая.\n\n" +
                        "ОЦЕНКА СЛОЖНОСТИ:\n" +
                        "• В худшем и среднем случаях: O(log n)\n" +
                        "• В лучшем случае (искомый элемент в центре): O(1)\n" +
                        "• Дополнительная память: O(1) при итеративной реализации.\n\n" +
                        "ПРАВИЛА ПРОХОЖДЕНИЯ:\n" +
                        "Открывайте сундук строго в середине активного диапазона. За линейный перебор начисляется штраф.",
                    InitialData = "4,8,15,16,23,42,55,64",
                    TargetValue = "23",
                    MaxScore = 120
                },
                new LevelInfo
                {
                    Id = 5,
                    Title = "Стек и Очередь (LIFO / FIFO)",
                    Category = LevelCategories.DataStructures,
                    ShortGoal = "Выполните погрузку и разгрузка трюма по правилам стека (LIFO) и очереди (FIFO).",
                    TheoryText =
                        "ПРИНЦИП РАБОТЫ:\n" +
                        "• Стек (LIFO — Last In, First Out): последним пришел — первым вышел. Операции Push (добавить) " +
                        "и Pop (извлечь сверху).\n" +
                        "• Очередь (FIFO — First In, First Out): первым пришел — первым вышел. Операции Enqueue (в конец) " +
                        "и Dequeue (из начала).\n\n" +
                        "ОЦЕНКА СЛОЖНОСТИ:\n" +
                        "• Добавление и извлечение элемента в стеке и очереди: O(1)\n" +
                        "• Память: O(n), где n — количество хранимых элементов.\n\n" +
                        "ПРАВИЛА ПРОХОЖДЕНИЯ:\n" +
                        "Разместите контейнеры в трюме и извлеките их в порядке, требуемом выбранным режимом (LIFO или FIFO).",
                    InitialData = "10,20,30,40",
                    TargetValue = "40,30,20,10",
                    MaxScore = 120
                },
                new LevelInfo
                {
                    Id = 6,
                    Title = "Автопилот вагонетки",
                    Category = LevelCategories.Executor,
                    ShortGoal = "Составьте программу команд (Вверх, Вниз, Влево, Вправо) для проезда от S до E по сетке 5x5.",
                    TheoryText =
                        "ПРИНЦИП РАБОТЫ:\n" +
                        "Исполнитель выполняет линейную последовательность дискретных команд перемещения по двумерной сетке. " +
                        "Поиск оптимального пути на сетке лежит в основе алгоритмов обхода графов (BFS / A*).\n\n" +
                        "ОЦЕНКА СЛОЖНОСТИ:\n" +
                        "• Выполнение готовой программы из k команд: O(k)\n" +
                        "• Поиск кратчайшего пути на сетке V×V (BFS): O(V²).\n\n" +
                        "ОБОЗНАЧЕНИЯ КАРТЫ 5x5:\n" +
                        "S — старт вагонетки, E — финиш (станция), . — свободный путь, # — завал (препятствие).\n\n" +
                        "ПРАВИЛА ПРОХОЖДЕНИЯ:\n" +
                        "Сформируйте программу движения без столкновений со стенами и выходов за границы поля.",
                    InitialData = "S,.,#,.,.;.,.,#,.,.;.,.,.,.,.;#,.,#,.,.;.,.,.,.,E",
                    MaxScore = 150
                }
            };
        }

        #endregion

        #region Экспорт сертификата успеваемости

        public void ExportCertificate(UserProfile profile, string filePath)
        {
            if (profile == null)
            {
                throw new ArgumentNullException("profile");
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("Путь к файлу сертификата не может быть пустым.", "filePath");
            }

            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            List<LevelInfo> levels = LoadLevels();
            string extension = Path.GetExtension(filePath);

            string content = string.Equals(extension, ".txt", StringComparison.OrdinalIgnoreCase)
                ? BuildTextCertificate(profile, levels)
                : BuildHtmlCertificate(profile, levels);

            File.WriteAllText(filePath, content, Encoding.UTF8);
        }

        private string BuildHtmlCertificate(UserProfile profile, List<LevelInfo> levels)
        {
            int totalLevels = levels.Count;
            int completedCount = 0;
            int maxPossibleScore = levels.Sum(l => l.MaxScore);

            var rowsBuilder = new StringBuilder();
            foreach (LevelInfo level in levels)
            {
                LevelResult result = profile.GetLevelResult(level.Id);
                bool isCompleted = result != null && result.IsCompleted;
                if (isCompleted)
                {
                    completedCount++;
                }

                string statusBadge = isCompleted
                    ? "<span class=\"badge badge-ok\">Пройден</span>"
                    : "<span class=\"badge badge-no\">Не пройден</span>";

                string bestScore = result != null ? result.BestScore.ToString() : "0";
                string bestTime = (isCompleted && result.BestTimeSeconds > 0)
                    ? string.Format("{0} сек.", result.BestTimeSeconds)
                    : "—";

                rowsBuilder.AppendLine("<tr>");
                rowsBuilder.AppendFormat("  <td>{0}</td>\n", level.Id);
                rowsBuilder.AppendFormat("  <td><strong>{0}</strong><br><small>{1}</small></td>\n",
                    WebUtility.HtmlEncode(level.Title),
                    WebUtility.HtmlEncode(level.Category));
                rowsBuilder.AppendFormat("  <td>{0}</td>\n", statusBadge);
                rowsBuilder.AppendFormat("  <td class=\"num\">{0} / {1}</td>\n", bestScore, level.MaxScore);
                rowsBuilder.AppendFormat("  <td class=\"num\">{0}</td>\n", bestTime);
                rowsBuilder.AppendLine("</tr>");
            }

            int progressPercent = totalLevels > 0 ? (int)Math.Round(completedCount * 100.0 / totalLevels) : 0;
            string grade = CalculateGrade(completedCount, totalLevels, profile.TotalScore, maxPossibleScore);
            string generatedAt = DateTime.Now.ToString("dd.MM.yyyy HH:mm");

            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang=\"ru\">");
            html.AppendLine("<head>");
            html.AppendLine("  <meta charset=\"UTF-8\">");
            html.AppendLine("  <title>Сертификат успеваемости — AlgoQuest</title>");
            html.AppendLine("  <style>");
            html.AppendLine("    body { font-family: 'Segoe UI', Arial, sans-serif; background: #f3f5f9; color: #1f2937; margin: 0; padding: 32px; }");
            html.AppendLine("    .certificate { max-width: 860px; margin: 0 auto; background: #ffffff; border: 2px solid #1e3a8a; border-radius: 12px; padding: 36px 42px; box-shadow: 0 10px 30px rgba(30, 58, 138, 0.12); }");
            html.AppendLine("    .header { text-align: center; border-bottom: 2px solid #e5e7eb; padding-bottom: 20px; margin-bottom: 24px; }");
            html.AppendLine("    .header h1 { margin: 0 0 8px; color: #1e3a8a; font-size: 28px; letter-spacing: 0.5px; }");
            html.AppendLine("    .header p { margin: 0; color: #6b7280; font-size: 14px; }");
            html.AppendLine("    .summary { display: flex; justify-content: space-between; gap: 16px; margin-bottom: 24px; flex-wrap: wrap; }");
            html.AppendLine("    .card { flex: 1; min-width: 170px; background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; padding: 14px 16px; }");
            html.AppendLine("    .card-label { font-size: 12px; color: #64748b; text-transform: uppercase; margin-bottom: 4px; }");
            html.AppendLine("    .card-value { font-size: 20px; font-weight: 700; color: #0f172a; }");
            html.AppendLine("    .progress-wrap { background: #e2e8f0; border-radius: 999px; height: 12px; overflow: hidden; margin-bottom: 24px; }");
            html.AppendLine("    .progress-bar { background: linear-gradient(90deg, #2563eb, #10b981); height: 100%; }");
            html.AppendLine("    table { width: 100%; border-collapse: collapse; margin-bottom: 28px; }");
            html.AppendLine("    th, td { border-bottom: 1px solid #e5e7eb; padding: 12px 10px; text-align: left; font-size: 14px; }");
            html.AppendLine("    th { background: #eff6ff; color: #1e3a8a; font-weight: 600; }");
            html.AppendLine("    td.num { text-align: right; font-variant-numeric: tabular-nums; }");
            html.AppendLine("    small { color: #6b7280; }");
            html.AppendLine("    .badge { display: inline-block; padding: 3px 10px; border-radius: 999px; font-size: 12px; font-weight: 600; }");
            html.AppendLine("    .badge-ok { background: #dcfce7; color: #166534; }");
            html.AppendLine("    .badge-no { background: #fee2e2; color: #991b1b; }");
            html.AppendLine("    .footer { display: flex; justify-content: space-between; align-items: center; border-top: 1px solid #e5e7eb; padding-top: 16px; font-size: 13px; color: #6b7280; }");
            html.AppendLine("    @media print { body { background: #fff; padding: 0; } .certificate { box-shadow: none; } }");
            html.AppendLine("  </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("  <div class=\"certificate\">");
            html.AppendLine("    <div class=\"header\">");
            html.AppendLine("      <h1>СЕРТИФИКАТ УСПЕВАЕМОСТИ</h1>");
            html.AppendLine("      <p>Учебный тренажер алгоритмов и структур данных</p>");
            html.AppendLine("    </div>");
            html.AppendLine("    <div class=\"summary\">");
            html.AppendFormat("      <div class=\"card\"><div class=\"card-label\">Студент</div><div class=\"card-value\">{0}</div></div>\n", WebUtility.HtmlEncode(profile.PlayerName));
            html.AppendFormat("      <div class=\"card\"><div class=\"card-label\">Общий счет</div><div class=\"card-value\">{0} / {1}</div></div>\n", profile.TotalScore, maxPossibleScore);
            html.AppendFormat("      <div class=\"card\"><div class=\"card-label\">Пройдено уровней</div><div class=\"card-value\">{0} из {1} ({2}%)</div></div>\n", completedCount, totalLevels, progressPercent);
            html.AppendFormat("      <div class=\"card\"><div class=\"card-label\">Итоговая оценка</div><div class=\"card-value\">{0}</div></div>\n", WebUtility.HtmlEncode(grade));
            html.AppendLine("    </div>");
            html.AppendFormat("    <div class=\"progress-wrap\"><div class=\"progress-bar\" style=\"width: {0}%;\"></div></div>\n", progressPercent);
            html.AppendLine("    <table>");
            html.AppendLine("      <thead>");
            html.AppendLine("        <tr><th>№</th><th>Уровень и категория</th><th>Статус</th><th style=\"text-align:right\">Лучший счет</th><th style=\"text-align:right\">Мин. время</th></tr>");
            html.AppendLine("      </thead>");
            html.AppendLine("      <tbody>");
            html.Append(rowsBuilder.ToString());
            html.AppendLine("      </tbody>");
            html.AppendLine("    </table>");
            html.AppendLine("    <div class=\"footer\">");
            html.AppendFormat("      <span>Дата формирования: {0}</span>\n", generatedAt);
            html.AppendLine("      <span>Подпись преподавателя: ______________________</span>");
            html.AppendLine("    </div>");
            html.AppendLine("  </div>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        private string BuildTextCertificate(UserProfile profile, List<LevelInfo> levels)
        {
            int completedCount = 0;
            int maxPossibleScore = levels.Sum(l => l.MaxScore);
            var sb = new StringBuilder();

            sb.AppendLine("=================================================================");
            sb.AppendLine("             СЕРТИФИКАТ УСПЕВАЕМОСТИ СТУДЕНТА");
            sb.AppendLine("=================================================================");
            sb.AppendFormat("Студент:           {0}\r\n", profile.PlayerName);
            sb.AppendFormat("Дата формирования: {0:dd.MM.yyyy HH:mm}\r\n", DateTime.Now);
            sb.AppendFormat("Общий счет:        {0} / {1}\r\n", profile.TotalScore, maxPossibleScore);
            sb.AppendLine("-----------------------------------------------------------------");

            foreach (LevelInfo level in levels)
            {
                LevelResult result = profile.GetLevelResult(level.Id);
                bool isCompleted = result != null && result.IsCompleted;
                if (isCompleted)
                {
                    completedCount++;
                }

                string status = isCompleted ? "Пройден" : "Не пройден";
                int score = result != null ? result.BestScore : 0;
                string time = (isCompleted && result.BestTimeSeconds > 0)
                    ? string.Format("{0} сек.", result.BestTimeSeconds)
                    : "—";

                sb.AppendFormat("{0}. {1} [{2}]\r\n", level.Id, level.Title, level.Category);
                sb.AppendFormat("   Статус: {0} | Лучший счет: {1}/{2} | Мин. время: {3}\r\n",
                    status, score, level.MaxScore, time);
            }

            string grade = CalculateGrade(completedCount, levels.Count, profile.TotalScore, maxPossibleScore);
            sb.AppendLine("-----------------------------------------------------------------");
            sb.AppendFormat("Пройдено уровней:  {0} из {1}\r\n", completedCount, levels.Count);
            sb.AppendFormat("Итоговая оценка:   {0}\r\n", grade);
            sb.AppendLine("=================================================================");

            return sb.ToString();
        }

        private static string CalculateGrade(int completedCount, int totalLevels, int totalScore, int maxPossibleScore)
        {
            if (totalLevels == 0 || completedCount == 0)
            {
                return "Не аттестован";
            }

            double completionRatio = (double)completedCount / totalLevels;
            double scoreRatio = maxPossibleScore > 0 ? (double)totalScore / maxPossibleScore : 0;

            if (completionRatio >= 1.0 && scoreRatio >= 0.85)
            {
                return "Отлично (5)";
            }

            if (completionRatio >= 0.8 && scoreRatio >= 0.65)
            {
                return "Хорошо (4)";
            }

            if (completionRatio >= 0.5)
            {
                return "Удовлетворительно (3)";
            }

            return "В процессе";
        }

        #endregion
    }
}
