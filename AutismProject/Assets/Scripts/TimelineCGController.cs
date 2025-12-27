using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using System.Collections;

public class TimelineCGController : MonoBehaviour
{
    [Header("UI 面板引用")]
    public GameObject startPanel;       
    public GameObject levelSelectPanel; 
    public GameObject interactionRoot;  

    [Header("互动子模块引用")]
    public GameObject headButton;       // P3 (Index 2)
    public GameObject stomachButton;    // P4 (Index 3)
    public GameObject choicePanelPage8; // P8 (Index 7)
    public GameObject micPanelPage13;   // P13 (Index 12)

    [Header("核心组件")]
    public Image cgDisplay;
    public PlayableDirector director;
    public AudioSource voSource;

    [Header("资源配置")]
    public Sprite[] allCGs;
    
    // 【新增】两个音频数组
    [Tooltip("关卡模式/互动模式下的语音 (对应 Index 2, 3, 7, 12 等)")]
    public AudioClip[] levelVoiceOvers; 
    
    [Tooltip("自动播放故事模式下的语音 (对应 Index 0-14)")]
    public AudioClip[] storyVoiceOvers; 
    
    [Header("自动播放设置")]
    [Tooltip("当语音播完后，需要等待几秒才自动切换下一张")]
    [Range(0f, 10f)] // 这会在面板上弄一个 0到10 的滑动条，很方便
    public float autoPlayInterval = 3.0f; // 默认值设为 3秒

    // --- 状态变量 ---
    private int currentIndex = 0;
    private bool isAutoStoryMode = false; 
    public bool isInteractMode = false;   

    void Start()
    {
        startPanel.SetActive(true);
        levelSelectPanel.SetActive(false);
        interactionRoot.SetActive(false); 
        HideAllInteractionUI();
        if (allCGs.Length > 0) cgDisplay.sprite = allCGs[0];
    }

    // ==========================================
    //              第一部分：自动播放
    // ==========================================

    public void OnStartGameClick()
    {
        startPanel.SetActive(false);
        isAutoStoryMode = true;
        interactionRoot.SetActive(false);
        StartCoroutine(AutoPlayStoryRoutine());
    }

    IEnumerator AutoPlayStoryRoutine()
    {
        for (int i = 0; i < allCGs.Length; i++)
        {
            if (!isAutoStoryMode) yield break;

            currentIndex = i;

            // --- 【核心修改开始】 ---
            if (i == 0)
            {
                //如果是第一张图，我们不要用 Timeline（因为 Timeline 包含淡出）
                //我们手动让它从黑变白（只淡入）
                
                // 1. 确保是第一张图
                cgDisplay.sprite = allCGs[0];
                
                // 2. 播放语音
                PlayCurrentVoiceOver();

                // 3. 手动淡入 (黑 -> 白，耗时1秒)
                float timer = 0f;
                while (timer < 1f)
                {
                    timer += Time.deltaTime;
                    cgDisplay.color = Color.Lerp(Color.black, Color.white, timer);
                    yield return null;
                }
                cgDisplay.color = Color.white; // 确保彻底变白
            }
            else
            {
                // 如果是第2,3,4...张图，正常走 Timeline (淡出->换图->淡入)
                if (director != null) director.Play();

                yield return new WaitForSeconds(0.5f); // 等淡出
                // OnSwapSignal 触发换图+播音
                yield return new WaitForSeconds(0.5f); // 等淡入
            }
            // --- 【核心修改结束】 ---

            // 1. 基础等待时间 = 你设置的变量
            float waitTime = autoPlayInterval; 

            // 2. 如果有语音，等待时间 = 语音长度 + 你设置的变量
            if (currentIndex < storyVoiceOvers.Length && storyVoiceOvers[currentIndex] != null)
            {
                waitTime = storyVoiceOvers[currentIndex].length + autoPlayInterval;
            }

            // 3. 开始等待
            yield return new WaitForSeconds(waitTime);
        }

        ReturnToLevelSelect();
    }

    // ==========================================
    //              第二部分：关卡流程
    // ==========================================

    public void ReturnToLevelSelect()
    {
        StopAllCoroutines(); 
        isAutoStoryMode = false;
        voSource.Stop(); 
        HideAllInteractionUI();
        interactionRoot.SetActive(false);
        cgDisplay.color = Color.black; 
        levelSelectPanel.SetActive(true); 
    }

    public void StartLevel(int targetIndex)
    {
        levelSelectPanel.SetActive(false);
        isAutoStoryMode = false; 
        interactionRoot.SetActive(true); 

        // 进入指定关卡
        JumpToPage(targetIndex);
    }

    // 【新增】封装一个跳转页面的函数，方便复用
    void JumpToPage(int index)
    {
        currentIndex = index;

        // 1. 换图
        if (currentIndex < allCGs.Length)
        {
            cgDisplay.sprite = allCGs[currentIndex];
            cgDisplay.color = Color.white; 
        }

        // 2. 播音 (会自动判断模式选择音频)
        PlayCurrentVoiceOver();

        // 3. 触发互动
        CheckForSpecialEvents();
    }

    // ==========================================
    //              第三部分：音频与互动
    // ==========================================

    public void OnSwapSignal()
    {
        if (isAutoStoryMode)
        {
            if (currentIndex < allCGs.Length) cgDisplay.sprite = allCGs[currentIndex];
            PlayCurrentVoiceOver();
        }
    }

    // 【核心修改】根据模式选择播放哪个数组的音频
    void PlayCurrentVoiceOver()
    {
        AudioClip clipToPlay = null;

        if (isAutoStoryMode)
        {
            // 自动模式：用 Story 数组
            if (currentIndex < storyVoiceOvers.Length) 
                clipToPlay = storyVoiceOvers[currentIndex];
        }
        else
        {
            // 关卡模式：用 Level 数组
            if (currentIndex < levelVoiceOvers.Length) 
                clipToPlay = levelVoiceOvers[currentIndex];
        }

        if (clipToPlay != null)
        {
            voSource.clip = clipToPlay;
            voSource.Play();
        }
    }

    public void CheckForSpecialEvents()
    {
        if (isAutoStoryMode) return;

        Debug.Log($"检测互动 Page: {currentIndex}");

        if (currentIndex == 2) 
            StartCoroutine(ShowButtonAfterVoice(headButton));
        else if (currentIndex == 3) 
            StartCoroutine(ShowButtonAfterVoice(stomachButton));
        else if (currentIndex == 7) 
            StartCoroutine(ShowButtonAfterVoice(choicePanelPage8));
        else if (currentIndex == 12) 
            StartCoroutine(ShowButtonAfterVoice(micPanelPage13));
    }

    IEnumerator ShowButtonAfterVoice(GameObject targetButton)
    {
        isInteractMode = true; 
        float waitTime = 0f;
        if (voSource.clip != null) waitTime = voSource.clip.length;
        
        yield return new WaitForSeconds(waitTime + 0.2f);
        if (targetButton != null) targetButton.SetActive(true);
    }

    public void FinishInteraction()
    {
        isInteractMode = false;
    }

    // 【核心修改】解决第3页直接跳出的问题
    public void ForceNextPage()
    {
        if (isAutoStoryMode) return;

        // --- 关卡连贯性逻辑 ---
        
        if (currentIndex == 2) 
        {
            // 如果是在第3页（摸头），做完后应该去第4页（捂肚子）
            Debug.Log("摸头完成，自动进入下一页（捂肚子）");
            JumpToPage(3); // 手动跳到 Index 3
        }
        else
        {
            // 其他情况（比如第4页做完，第8页做完，第13页做完），直接回菜单
            Debug.Log("关卡结束，返回菜单");
            Invoke("ReturnToLevelSelect", 1.0f);
        }
    }

    void HideAllInteractionUI()
    {
        if(headButton) headButton.SetActive(false);
        if(stomachButton) stomachButton.SetActive(false);
        if(choicePanelPage8) choicePanelPage8.SetActive(false);
        if(micPanelPage13) micPanelPage13.SetActive(false);
    }
}