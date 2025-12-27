using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BodyPartClick : MonoBehaviour
{
    [Header("必须配置")]
    public TimelineCGController controller; 
    public AudioSource audioSource;         
    public AudioClip feedbackVoice;         

    private bool isClicked = false;
    private Image myImage;
    private Animator myAnimator;
    
    // 【新增】用来记住原本的黄色
    private Color originalColor; 

    void Awake() // 改用 Awake 确保最早执行
    {
        myImage = GetComponent<Image>();
        myAnimator = GetComponent<Animator>();
        GetComponent<Button>().onClick.AddListener(OnClickBodyPart);

        // 记住一开始在 Inspector 里设置的颜色
        if (myImage != null) originalColor = myImage.color;
    }

    void OnEnable()
    {
        isClicked = false;
        if (myAnimator != null) myAnimator.enabled = true; // 恢复呼吸
        
        // 【核心修复】强制恢复成原本的颜色（黄色），而不是白色或绿色
        if (myImage != null) myImage.color = originalColor; 
    }

    void OnClickBodyPart()
    {
        if (isClicked) return;
        isClicked = true;
        StartCoroutine(FeedbackRoutine());
    }

    IEnumerator FeedbackRoutine()
    {
        if (myAnimator != null) myAnimator.enabled = false;
        if (myImage != null) myImage.color = Color.green; // 变绿

        float waitTime = 1.0f;
        if (audioSource != null && feedbackVoice != null)
        {
            audioSource.PlayOneShot(feedbackVoice);
            waitTime = feedbackVoice.length;
        }

        yield return new WaitForSeconds(waitTime + 0.5f);

        if (controller != null)
        {
            controller.FinishInteraction(); 
            controller.ForceNextPage();     
        }
        
        gameObject.SetActive(false);
    }
}