using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class CountdownTextUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _countdownText;
    [SerializeField] private float _countdownTime = 5f;
    private float _countdown;

    public static event Action OnCountDownFinished;

    public void StartCountdown(int countdown)
    {
        _countdownText.gameObject.SetActive(true);
        _countdownText.text = countdown.ToString();

        _countdown = countdown;
        StartCoroutine(Start_Countdown());
    }

    private IEnumerator Start_Countdown()
    {
        

        while (_countdown > 0)
        {
            _countdownText.text = Mathf.Ceil(_countdown).ToString();
            _countdownText.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 1);
            yield return new WaitForSeconds(1f);
            _countdown--;
        }

        _countdownText.text = "GO!";

        yield return new WaitForSeconds(1f);
        _countdownText.gameObject.SetActive(false);
        OnCountDownFinished?.Invoke();
    }
}
