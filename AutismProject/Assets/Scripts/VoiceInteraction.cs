using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Android;

public class VoiceInteraction : MonoBehaviour
{
    [Header("配置")]
    public TimelineCGController mainController; // 拖入 GameManager
    public Image volumeBar;           // 拖入那个 Fill Type 的绿色能量条
    public float sensitivity = 100f;  // 灵敏度（声音太小就调大这个数）
    public float threshold = 0.5f;    // 阈值：音量条超过多少算成功 (0到1之间)
    
    [Header("成功反馈")]
    public AudioSource sfxSource;     // 播放音效的 AudioSource
    public AudioClip successClip;     // 成功的音效（叮咚/欢呼）
    public GameObject successIcon;    // (可选) 成功时显示的图标，比如大拇指

    private AudioClip micClip;        // 录音片段
    private string deviceName;        // 麦克风设备名
    private bool isSuccess = false;   // 防止重复触发

    void Start()
    {
        Application.RequestUserAuthorization(UserAuthorization.Microphone);
        
        #if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Permission.RequestUserPermission(Permission.Microphone);
            }
        #endif
    }

    void OnEnable()
    {
        isSuccess = false;
        if (successIcon) successIcon.SetActive(false);
        StartMicrophone();
    }

    void OnDisable()
    {
        StopMicrophone();
    }

    void Update()
    {
        if (isSuccess) return;

        // 1. 获取当前音量 (0 ~ 1)
        float volume = GetLoudnessFromMic() * sensitivity;

        // 2. 视觉反馈：把音量显示在 UI 条上
        // 使用 Lerp 让条子跳动得平滑一点，不那么闪烁
        if (volumeBar != null)
        {
            volumeBar.fillAmount = Mathf.Lerp(volumeBar.fillAmount, volume, Time.deltaTime * 10);
        }

        // 3. 判定逻辑：只要有一瞬间声音够大
        if (volume > threshold)
        {
            StartCoroutine(SuccessRoutine());
        }
    }

    // --- 核心：开启麦克风 ---
    void StartMicrophone()
    {
        // 获取第一个可用的麦克风
        if (Microphone.devices.Length > 0)
        {
            deviceName = Microphone.devices[0];
            // 开始录音：循环模式(true)，时长10秒(够循环了)，采样率44100
            micClip = Microphone.Start(deviceName, true, 10, 44100);
        }
        else
        {
            Debug.LogError("没有检测到麦克风！直接跳过...");
            // 如果没麦克风，为了不卡死，直接算成功
            StartCoroutine(SuccessRoutine());
        }
    }

    void StopMicrophone()
    {
        Microphone.End(deviceName);
    }

    // --- 核心：分析音量算法 ---
    float GetLoudnessFromMic()
    {
        if (micClip == null) return 0;

        // 现在的录音头位置
        int decPosition = Microphone.GetPosition(deviceName) - 128 + 1;
        if (decPosition < 0) return 0;

        // 取最近的 128 个采样点
        float[] waveData = new float[128];
        micClip.GetData(waveData, decPosition);

        // 计算平均振幅 (RMS)
        float levelMax = 0;
        for (int i = 0; i < 128; i++)
        {
            float wavePeak = waveData[i] * waveData[i];
            if (levelMax < wavePeak)
            {
                levelMax = wavePeak;
            }
        }
        return Mathf.Sqrt(levelMax); // 返回音量值
    }

    IEnumerator SuccessRoutine()
    {
        isSuccess = true;
        Debug.Log("声音检测成功！");

        // 1. 视觉反馈：瞬间把能量条填满，表示爆表
        if (volumeBar) volumeBar.fillAmount = 1f;
        if (successIcon) successIcon.SetActive(true);

        // 2. 听觉反馈
        if (sfxSource && successClip) sfxSource.PlayOneShot(successClip);

        // 3. 稍作停留，给孩子一点成就感
        yield return new WaitForSeconds(1.5f);

        // 4. 关闭自己
        gameObject.SetActive(false);

        // 5. 翻页
        mainController.FinishInteraction();
        mainController.ForceNextPage();
    }
}