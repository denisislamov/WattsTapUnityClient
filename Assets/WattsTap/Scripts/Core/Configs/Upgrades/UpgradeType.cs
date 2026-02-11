namespace WattsTap.Scripts.Game.GlobalConfigs
{
    /// <summary>
    /// Типы доступных улучшений
    /// </summary>
    public enum UpgradeType
    {
        GoldHammer,         // Увеличивает профит от каждого удара
        FastTime,           // Уменьшает кулдаун каждого удара
        Endurance,          // Увеличивает вместимость ударов
        CriticalChance,     // Увеличивает шанс получения x2 профита с удара
        CriticalMultiplier, // Увеличивает множитель критического удара
        WorkExperience,     // Увеличивает опыт от каждого удара
        Economist,          // Уменьшает цену апгрейдов
        StrongFriendship,   // Увеличивает бонус от друзей
        Investor,           // Увеличивает доход в час
        ItemMaster,         // Уменьшает цену предметов для повышения уровня
        ShareProfit,        // Увеличивает награду монетами от % профита друзей
        GoldFriends         // Увеличивает награду монетами за приглашенного друга
    }
}