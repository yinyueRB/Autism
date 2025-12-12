using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BodyPartClick : MonoBehaviour
{
    [Header("必须配置")]
    public TimelineCGController controller; // 拖入 GameManager
    public AudioSource audioSource;         // 拖入挂在自己身上的 AudioSource 组件
    public AudioClip feedbackVoice;         // 拖入 mp3 文件

    private bool isClicked = false;
    private Image myImage;
    private Animator myAnimator;

    void Start()
    {
        // 获取自身的组件
        myImage = GetComponent<Image>();
        myAnimator = GetComponent<Animator>();

        // 绑定点击事件
        GetComponent<Button>().onClick.AddListener(OnClickBodyPart);
    }

    // 每次物体重新显示时（比如重玩），重置状态
    void OnEnable()
    {
        isClicked = false;
        if (myAnimator != null) myAnimator.enabled = true; // 重新开始呼吸闪烁
        if (myImage != null) myImage.color = new Color(1, 1, 1, 0.5f); // 恢复成半透明白色
    }

    void OnClickBodyPart()
    {
        if (isClicked) return; // 防止重复点击
        isClicked = true;

        StartCoroutine(FeedbackRoutine());
    }

    IEnumerator FeedbackRoutine()
    {
        // 1. 立刻停止闪烁（关键步骤）
        // 必须禁用的 Animator，否则它会覆盖我们下面设置的颜色
        if (myAnimator != null)
        {
            myAnimator.enabled = false; 
        }

        // 2. 变绿，且设置为完全不透明
        // new Color(R, G, B, A) -> (0, 1, 0, 1) 代表纯绿，不透明
        if (myImage != null)
        {
            myImage.color = new Color(0f, 1f, 0f, 0.8f); 
        }

        // 3. 播放声音
        if (audioSource != null && feedbackVoice != null)
        {
            audioSource.PlayOneShot(feedbackVoice);
        }

        // 4. 停留 1 秒钟
        yield return new WaitForSeconds(1.0f);

        // 5. 告诉控制器解锁，允许切下一页
        if (controller != null)
        {
            controller.FinishInteraction();
        }

        // 6. 消失 (把自己关掉)
        gameObject.SetActive(false);
    }
}
