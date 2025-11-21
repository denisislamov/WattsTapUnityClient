using System;
using UnityEngine;
using WattsTap.Core.React;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class WelcomeBackUIModel : UIBaseModel
    {
        private static readonly TimeSpan DefaultTotalDuration = TimeSpan.FromHours(12);
        private static readonly TimeSpan DefaultRemainingDuration = new(3, 10, 52);

        public ReactiveProperty<TimeSpan> RemainingTime { get; private set; }
        public ReactiveProperty<float> TimerProgress { get; private set; }
        public ReactiveProperty<long> RewardAmount { get; private set; }

        public TimeSpan TotalDuration => DefaultTotalDuration;

        public override void Initialize()
        {
            base.Initialize();

            RemainingTime = new ReactiveProperty<TimeSpan>(DefaultRemainingDuration);
            TimerProgress = new ReactiveProperty<float>(CalculateProgress(DefaultRemainingDuration));
            RewardAmount = new ReactiveProperty<long>(0);
        }

        public void ApplyRemainingTime(TimeSpan newRemainingTime)
        {
            if (newRemainingTime < TimeSpan.Zero)
            {
                newRemainingTime = TimeSpan.Zero;
            }
            else if (newRemainingTime > TotalDuration)
            {
                newRemainingTime = TotalDuration;
            }

            RemainingTime.Value = newRemainingTime;
            TimerProgress.Value = CalculateProgress(newRemainingTime);
        }

        public void SetRewardAmount(long amount)
        {
            RewardAmount.Value = amount;
        }

        private float CalculateProgress(TimeSpan remainingTime)
        {
            var elapsed = TotalDuration - remainingTime;
            var totalSeconds = (float)TotalDuration.TotalSeconds;

            return totalSeconds <= 0f ? 1f : Mathf.Clamp01((float)elapsed.TotalSeconds / totalSeconds);
        }

        public override void Dispose()
        {
            RemainingTime?.Dispose();
            TimerProgress?.Dispose();
            RewardAmount?.Dispose();

            base.Dispose();
        }
    }
}

