using System;
using System.Collections;
using UnityEngine;
using WattsTap.Constants;
using WattsTap.Core;
using WattsTap.Core.Telegram;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class WelcomeBackUIPresenter : UIBasePresenter<WelcomeBackUIView, WelcomeBackUIModel>
    {
        private const float TickIntervalSeconds = 1f;

        private Coroutine _timerCoroutine;
        private IUIService _uiService;
        private IHapticFeedbackService _hapticService;

        protected override void OnInit()
        {
            Model.RemainingTime.OnValueChanged += OnRemainingTimeChanged;
            Model.TimerProgress.OnValueChanged += OnTimerProgressChanged;
            Model.RewardAmount.OnValueChanged += OnRewardAmountChanged;
            
            ServiceLocator.TryGet(out _hapticService);

            if (View.CloseButton != null)
            {
                View.CloseButton.onClick.AddListener(OnCloseClicked);
            }

            OnRemainingTimeChanged(Model.RemainingTime.Value);
            OnTimerProgressChanged(Model.TimerProgress.Value);
            OnRewardAmountChanged(Model.RewardAmount.Value);

            StartMockTimer();
        }

        private void StartMockTimer()
        {
            // TODO: Replace mock timer with real first-return timer logic.
            StopMockTimer();
            _timerCoroutine = View.StartCoroutine(MockTimerRoutine());
        }

        private void StopMockTimer()
        {
            if (_timerCoroutine != null)
            {
                View.StopCoroutine(_timerCoroutine);
                _timerCoroutine = null;
            }
        }

        private IEnumerator MockTimerRoutine()
        {
            var tickStep = TimeSpan.FromSeconds(TickIntervalSeconds);

            while (Model.RemainingTime.Value > TimeSpan.Zero)
            {
                yield return new WaitForSeconds(TickIntervalSeconds);
                var nextValue = Model.RemainingTime.Value - tickStep;
                Model.ApplyRemainingTime(nextValue);
            }
        }

        private void OnRemainingTimeChanged(TimeSpan remainingTime)
        {
            View.SetTimerText(remainingTime);
        }

        private void OnTimerProgressChanged(float progress)
        {
            View.SetTimerProgress(progress);
        }

        private void OnRewardAmountChanged(long amount)
        {
            View.SetRewardValue(amount);
        }

        private void OnCloseClicked()
        {
            _hapticService?.ButtonPressed();
            
            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            _uiService?.Close(UIConstants.WelcomeBackScreen);
        }

        protected override void OnDispose()
        {
            StopMockTimer();

            if (Model != null)
            {
                Model.RemainingTime.OnValueChanged -= OnRemainingTimeChanged;
                Model.TimerProgress.OnValueChanged -= OnTimerProgressChanged;
                Model.RewardAmount.OnValueChanged -= OnRewardAmountChanged;
            }

            if (View?.CloseButton != null)
            {
                View.CloseButton.onClick.RemoveListener(OnCloseClicked);
            }
        }
    }
}
