using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SocialChoice : MonoBehaviour
{
    [Header("核心引用")]
    public TimelineCGController mainController; // 拖入 GameManager
    public Button correctBtn;
    public Button wrongBtn;

    [Header("音频配置")]
    public AudioSource uiAudioSource; // 用来播放按钮声音的喇叭
    public AudioClip correctVoice;    // "老师我不舒服"
    public AudioClip wrongVoice;      // "我要玩玩具"
    public AudioClip successSFX;      // 叮咚！正确提示音

    [Header("动画")]
    public Animator wrongBtnAnimator; // 拖入 Btn_Wrong 上的 Animator

    private bool isHandlingClick = false; // 防止狂点

    void Start()
    {
        // 自动绑定点击事件
        correctBtn.onClick.AddListener(OnCorrectClick);
        wrongBtn.onClick.AddListener(OnWrongClick);
    }

    void OnEnable()
    {
        isHandlingClick = false; // 每次面板显示时重置状态
    }

    // --- 选中正确选项 ---
    void OnCorrectClick()
    {
        if (isHandlingClick) return;
        isHandlingClick = true; // 锁定，防止重复点

        StartCoroutine(CorrectRoutine());
    }

    IEnumerator CorrectRoutine()
    {
        // 1. 播放“老师我不舒服”
        uiAudioSource.clip = correctVoice;
        uiAudioSource.Play();

        // 2. 等待语音播完 (或者固定等几秒)
        yield return new WaitForSeconds(correctVoice.length);

        // 3. 播放正确音效 (叮咚!)
        uiAudioSource.PlayOneShot(successSFX);
        
        // 4. 等待音效稍微响一下
        yield return new WaitForSeconds(1.0f);

        // 5. 隐藏面板
        gameObject.SetActive(false);

        // 6. 【关键】告诉主控制器：强制进入下一页
        mainController.FinishInteraction(); // 解锁
        mainController.ForceNextPage();     // 强制切页
    }

    // --- 选中干扰选项 ---
    void OnWrongClick()
    {
        // 错误选项不需要锁死，可以让孩子反复点，直到选对
        // 1. 播放“我要玩玩具”
        uiAudioSource.Stop(); // 打断之前的声音
        uiAudioSource.PlayOneShot(wrongVoice);

        // 2. 触发晃动动画
        if (wrongBtnAnimator != null)
        {
            wrongBtnAnimator.SetTrigger("Shake");
        }

        // 3. 不做任何跳转，停留在当前页
    }
}
