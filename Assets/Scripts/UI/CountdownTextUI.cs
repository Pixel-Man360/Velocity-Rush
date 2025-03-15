using System;
using System.Collections;
using TMPro;
using UnityEngine;
using DG.Tweening;
using Fusion;

public class CountdownTextUI : NetworkBehaviour
{
    [SerializeField] private TMP_Text _countdownText;
    [SerializeField] private float _countdownTime = 5f;

    public static event Action OnCountDownFinished;

    public void StartCountdown(int countdown)
    {
        _countdownText.gameObject.SetActive(true);
        StartCoroutine(StartCountdownCoroutine(countdown));
    }

    private IEnumerator StartCountdownCoroutine(int countdown)
    {
        int currentCount = countdown;

        while (currentCount > 0)
        {
            UpdateCountdownUI(currentCount);
            yield return new WaitForSeconds(1f);
            currentCount--;
        }

        _countdownText.text = "GO!";
        OnCountDownFinished?.Invoke();
        yield return new WaitForSeconds(1f);
        _countdownText.gameObject.SetActive(false);
    }

    private void UpdateCountdownUI(int value)
    {
        _countdownText.text = value.ToString();
        _countdownText.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 1);
    }
}
