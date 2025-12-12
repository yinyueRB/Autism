using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using System.Collections; // 必须引用这个，用于协程

public class TimelineCGController : MonoBehaviour
{
    [Header("图像配置")]
    public Image cgDisplay;
    public Sprite[] allCGs;
    public PlayableDirector director;

    [Header("旁白音频配置")]
    public AudioSource voSource;    
    public AudioClip[] allVoiceOvers; 

    [Header("交互按钮配置")]
    public GameObject headButton;
    public GameObject stomachButton;

    private int currentIndex = 0;
    public bool isInteractMode = false;

    void Start()
    {
        if (allCGs.Length > 0) cgDisplay.sprite = allCGs[0];
        
        // 确保按钮隐藏
        if(headButton) headButton.SetActive(false);
        if(stomachButton) stomachButton.SetActive(false);

        PlayCurrentVoiceOver();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isInteractMode) return; // 交互模式中，点击无效（等待按按钮）
            if (voSource.isPlaying) return; // 旁白没播完，点击无效

            if (director.state != PlayState.Playing && currentIndex < allCGs.Length - 1)
            {
                director.Play(); 
            }
        }
    }

    public void OnSwapSignal()
    {
        currentIndex++; 

        // 1. 换图
        if (currentIndex < allCGs.Length)
        {
            cgDisplay.sprite = allCGs[currentIndex];
        }

        // 2. 换音频并播放
        PlayCurrentVoiceOver();

        // 3. 检查是否需要显示按钮（并启动延时逻辑）
        CheckForSpecialEvents();
    }

    void PlayCurrentVoiceOver()
    {
        if (currentIndex < allVoiceOvers.Length && allVoiceOvers[currentIndex] != null)
        {
            voSource.clip = allVoiceOvers[currentIndex]; 
            voSource.Play(); 
        }
    }

    // ---【重点修改在这里】---
    void CheckForSpecialEvents()
    {
        if (currentIndex == 2) // 第3页：摸头
        {
            // 开启协程：传入对应的按钮
            StartCoroutine(ShowButtonAfterVoice(headButton));
        }
        else if (currentIndex == 3) // 第4页：捂肚子
        {
            StartCoroutine(ShowButtonAfterVoice(stomachButton));
        }
    }

    // ---【新增的延时协程】---
    IEnumerator ShowButtonAfterVoice(GameObject targetButton)
    {
        // 1. 立刻锁定交互模式
        // 这样玩家既不能切页，也点不到还没出来的按钮（当然按钮也是藏着的）
        isInteractMode = true;

        // 2. 计算需要等待的时间
        // 如果当前有旁白在播，就等待它的长度；如果没有，就不用等
        float waitTime = 0f;
        if (voSource.clip != null)
        {
            waitTime = voSource.clip.length;
        }

        // 3. 开始等待（让旁白播完）
        // 这里加了0.2秒缓冲，让体验更自然，不那么急促
        yield return new WaitForSeconds(waitTime + 0.2f);

        // 4. 时间到！显示按钮
        if (targetButton != null)
        {
            targetButton.SetActive(true);
        }
    }

    public void FinishInteraction()
    {
        isInteractMode = false;
    }
}
