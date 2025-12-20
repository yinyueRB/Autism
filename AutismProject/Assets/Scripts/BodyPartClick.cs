using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BodyPartClick : MonoBehaviour
{
    [Header("必须配置")]
    public TimelineCGController controller; // 拖入 GameManager
    public AudioSource audioSource;         // 拖入挂在自己身上的 AudioSource
    public AudioClip feedbackVoice;         // 拖入 mp3 文件

    private bool isClicked = false;
    private Image myImage;
    private Animator myAnimator;

    void Start()
    {
        myImage = GetComponent<Image>();
        myAnimator = GetComponent<Animator>();
        GetComponent<Button>().onClick.AddListener(OnClickBodyPart);
    }

    void OnEnable()
    {
        isClicked = false;
        if (myAnimator != null) myAnimator.enabled = true; // 恢复呼吸
        if (myImage != null) myImage.color = new Color(1, 1, 1, 0.5f); // 恢复半透明
    }

    void OnClickBodyPart()
    {
        if (isClicked) return;
        isClicked = true;

        StartCoroutine(FeedbackRoutine());
    }

    IEnumerator FeedbackRoutine()
    {
        // 1. 停止闪烁，变绿
        if (myAnimator != null) myAnimator.enabled = false;
        if (myImage != null) myImage.color = new Color(0f, 1f, 0f, 0.8f); 

        // 2. 播放声音
        float waitTime = 1.0f; // 默认等1秒
        if (audioSource != null && feedbackVoice != null)
        {
            audioSource.PlayOneShot(feedbackVoice);
            waitTime = feedbackVoice.length; // 如果有声音，就改成等待声音的长度
        }

        // 3. 等待声音播完 (额外加0.5秒缓冲，让体验不那么急)
        yield return new WaitForSeconds(waitTime + 0.5f);

        // 4. 【核心修改】先解锁，然后直接强制翻页！
        if (controller != null)
        {
            controller.FinishInteraction(); // 解锁状态
            controller.ForceNextPage();     // <--- 这一句实现了自动跳转
        }

        // 5. 隐藏自己
        gameObject.SetActive(false);
    }
}
