namespace AachSavor.Models
{
    public static class RankHelper
    {
        /// <summary>
        /// Mốc điểm lên rank: 100, 500, 1000, 2000, 2500, 3500, 4500
        /// </summary>
        public static (string Rank, int NextTarget, int PrevTarget) GetRankInfo(int points)
        {
            if (points < 100) return ("A1", 100, 0);
            if (points < 500) return ("A2", 500, 100);
            if (points < 1000) return ("A3", 1000, 500);
            if (points < 2000) return ("A4", 2000, 1000);
            if (points < 2500) return ("A5", 2500, 2000);
            if (points < 3500) return ("A6", 3500, 2500);
            if (points < 4500) return ("A7", 4500, 3500);
            return ("A8", 4500, 4500); // Rank tối đa
        }

        public static string GetRankName(int points) => GetRankInfo(points).Rank;
    }
}
