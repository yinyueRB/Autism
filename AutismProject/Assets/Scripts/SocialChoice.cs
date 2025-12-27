using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SocialChoice : MonoBehaviour
{
    [Header("核心引用")]
    public TimelineCGController mainController; 
    public Button correctBtn;
    public Button wrongBtn;

    [Header("音频配置")]
    public AudioSource uiAudioSource; 
    public AudioClip correctVoice;    // "老师我不舒服"
    public AudioClip wrongVoice;      // "我要玩玩具"
    public AudioClip successSFX;      // 叮咚！正确提示音
    
    // 【新增】夸奖语音
    public AudioClip praiseVoice;     // "小朋友你做对啦"

    [Header("动画")]
    public Animator wrongBtnAnimator; 

    private bool isHandlingClick = false; 

    void Start()
    {
        correctBtn.onClick.AddListener(OnCorrectClick);
        wrongBtn.onClick.AddListener(OnWrongClick);
    }

    void OnEnable()
    {
        isHandlingClick = false; 
    }

    void OnCorrectClick()
    {
        if (isHandlingClick) return;
        isHandlingClick = true; 

        StartCoroutine(CorrectRoutine());
    }

    IEnumerator CorrectRoutine()
    {
        // 1. 播放“老师我不舒服”
        uiAudioSource.clip = correctVoice;
        uiAudioSource.Play();
        // 等待这句话说完
        yield return new WaitForSeconds(correctVoice.length);

        // 2. 播放正确音效 (叮咚!)
        uiAudioSource.PlayOneShot(successSFX);
        // 等待音效响一会儿 (比如1秒)
        yield return new WaitForSeconds(1.0f);

        // --- 【新增】播放夸奖语音 ---
        if (praiseVoice != null)
        {
            uiAudioSource.PlayOneShot(praiseVoice);
            // 等待夸奖说完，再多加0.5秒缓冲
            yield return new WaitForSeconds(praiseVoice.length + 0.5f);
        }
        else
        {
            // 如果没配置夸奖语音，就稍微等一下，别切太快
            yield return new WaitForSeconds(1.0f);
        }

        // 4. 结束
        gameObject.SetActive(false);
        mainController.FinishInteraction(); 
        mainController.ForceNextPage();     
    }

    void OnWrongClick()
    {
        uiAudioSource.Stop(); 
        uiAudioSource.PlayOneShot(wrongVoice);
        if (wrongBtnAnimator != null) wrongBtnAnimator.SetTrigger("Shake");
    }
}