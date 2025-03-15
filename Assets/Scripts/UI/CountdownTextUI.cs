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

    [Networked]  private float _countdown { get; set; }

    public static event Action OnCountDownFinished;

    public void StartCountdown(int countdown)
    {
        if (Object.HasStateAuthority)
        {
            _countdown = countdown;
            _countdownText.gameObject.SetActive(true);
            StartCoroutine(Start_Countdown());
        }
    }

    private IEnumerator Start_Countdown()
    {
        while (_countdown > 0)
        {
            UpdateCountdownUI();
            yield return new WaitForSeconds(1f);
            _countdown--;
        }

        _countdownText.text = "GO!";
        OnCountDownFinished?.Invoke();

        yield return new WaitForSeconds(1f);
        _countdownText.gameObject.SetActive(false);
    }

    private void UpdateCountdownUI()
    {
        _countdownText.text = Mathf.Ceil(_countdown).ToString();
        _countdownText.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 1);
    }
}
