using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android; // 保持安卓引用
using System.Collections;

public class VoiceInteraction : MonoBehaviour
{
    [Header("配置")]
    public TimelineCGController mainController; 
    public Image volumeBar;           
    public float sensitivity = 100f;  
    public float threshold = 0.5f;    
    
    [Header("成功反馈")]
    public AudioSource sfxSource;     
    public AudioClip successClip;     // 叮咚/欢呼
    
    // 【新增】夸奖语音
    public AudioClip praiseVoice;     // "小朋友你做对啦"
    
    public GameObject successIcon;    

    private AudioClip micClip;        
    private string deviceName;        
    private bool isSuccess = false;   

    void Start()
    {
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
        if (volumeBar) volumeBar.fillAmount = 0f; // 每次进入重置条子
        StartMicrophone();
    }

    void OnDisable()
    {
        StopMicrophone();
    }

    void Update()
    {
        if (isSuccess) return;

        float volume = GetLoudnessFromMic() * sensitivity;
        if (volumeBar != null)
        {
            volumeBar.fillAmount = Mathf.Lerp(volumeBar.fillAmount, volume, Time.deltaTime * 10);
        }

        if (volume > threshold)
        {
            StartCoroutine(SuccessRoutine());
        }
    }

    void StartMicrophone()
    {
        if (Microphone.devices.Length > 0)
        {
            deviceName = Microphone.devices[0];
            micClip = Microphone.Start(deviceName, true, 10, 44100);
        }
        else
        {
            // 没有麦克风的容错处理
            Debug.LogWarning("未检测到麦克风，直接跳过");
            StartCoroutine(SuccessRoutine());
        }
    }

    void StopMicrophone()
    {
        // 只有正在录音才停止，防止报错
        if (Microphone.IsRecording(deviceName))
        {
            Microphone.End(deviceName);
        }
    }

    float GetLoudnessFromMic()
    {
        if (micClip == null) return 0;
        int decPosition = Microphone.GetPosition(deviceName) - 128 + 1;
        if (decPosition < 0) return 0;
        float[] waveData = new float[128];
        micClip.GetData(waveData, decPosition);
        float levelMax = 0;
        for (int i = 0; i < 128; i++)
        {
            float wavePeak = waveData[i] * waveData[i];
            if (levelMax < wavePeak) levelMax = wavePeak;
        }
        return Mathf.Sqrt(levelMax); 
    }

    IEnumerator SuccessRoutine()
    {
        isSuccess = true;
        
        // 视觉反馈
        if (volumeBar) volumeBar.fillAmount = 1f;
        if (successIcon) successIcon.SetActive(true);

        // 1. 播放成功音效 (叮咚)
        if (sfxSource && successClip) 
        {
            sfxSource.PlayOneShot(successClip);
            // 等待音效响完 (假设音效1秒左右，或者你可以用 successClip.length)
            yield return new WaitForSeconds(1.0f);
        }

        // --- 【新增】播放夸奖语音 ---
        if (praiseVoice != null)
        {
            // 如果sfxSource也是用来播音效的，PlayOneShot可以混音
            sfxSource.PlayOneShot(praiseVoice);
            // 等待夸奖说完 + 缓冲
            yield return new WaitForSeconds(praiseVoice.length + 0.5f);
        }
        else
        {
             yield return new WaitForSeconds(1.0f);
        }

        // 结束
        gameObject.SetActive(false);
        mainController.FinishInteraction();
        mainController.ForceNextPage();
    }
}