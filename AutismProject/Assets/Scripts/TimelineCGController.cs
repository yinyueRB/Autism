using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables; // 必须引用 Timeline 命名空间

public class TimelineCGController : MonoBehaviour
{
    [Header("配置")]
    public Image cgDisplay;           // 显示图片的UI
    public Sprite[] allCGs;           // 15张图放这里
    public PlayableDirector director; // 引用 GameManager 上的 Director

    private int currentIndex = 0;

    void Start()
    {
        // 初始化第一张图
        if (allCGs.Length > 0)
        {
            cgDisplay.sprite = allCGs[0];
        }
    }

    void Update()
    {
        // 鼠标左键点击
        if (Input.GetMouseButtonDown(0))
        {
            // 核心逻辑：
            // 1. 如果 Timeline 正在播放，director.state 会是 Playing，这时候不许点。
            // 2. 只有当 Timeline 停止（由上一张图完全显示）时，才允许点。
            if (director.state != PlayState.Playing && currentIndex < allCGs.Length - 1)
            {
                director.Play(); // 播放 Timeline 动画
            }
        }
    }

    // 【重要】这个函数会被 Timeline 的 Signal 呼叫
    // 也就是在画面全黑的那一瞬间执行
    public void OnSwapSignal()
    {
        currentIndex++;
        if (currentIndex < allCGs.Length)
        {
            cgDisplay.sprite = allCGs[currentIndex];
        }
    }
}
